#include <vector>
#include <math.h>
#include <time.h>
#include <random>
#include <glm/gtc/matrix_transform.hpp>
#include "glwrap.h"
#include "camera.h"
#include "buildings.h"
#include "units.h"

static void error_callback(int error, const char* description)
{
	fprintf(stderr, "Error: %s\n", description);
}

void myCallback( GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, const GLchar *msg, const void *data )
{
	printf("GL Error: %s\n", msg);
}


Buildings GBuildings;
Units GUnits;
Camera GCamera;
double GDeltaTime;
uint32_t GUnitsNum = 32 * 1024;
uint32_t GFieldSizeX = 4096 + kStreetWidth;
uint32_t GFieldSizeY = 4096 + kStreetWidth;


void ProcessInput(GLFWwindow *window)
{
	if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS)
		glfwSetWindowShouldClose(window, true);

	float deltaTime = (float)GDeltaTime;
	if (glfwGetKey(window, GLFW_KEY_LEFT_SHIFT) == GLFW_PRESS)
		deltaTime *= 2.0f;

	if (glfwGetKey(window, GLFW_KEY_W) == GLFW_PRESS)
		GCamera.ProcessKeyboard(kCameraDirectionForward, deltaTime);
	if (glfwGetKey(window, GLFW_KEY_S) == GLFW_PRESS)
		GCamera.ProcessKeyboard(kCameraDirectionBackward, deltaTime);
	if (glfwGetKey(window, GLFW_KEY_A) == GLFW_PRESS)
		GCamera.ProcessKeyboard(kCameraDirectionLeft, deltaTime);
	if (glfwGetKey(window, GLFW_KEY_D) == GLFW_PRESS)
		GCamera.ProcessKeyboard(kCameraDirectionRight, deltaTime);
	if (glfwGetKey(window, GLFW_KEY_SPACE) == GLFW_PRESS)
		GCamera.ProcessKeyboard(kCameraDirectionUp, deltaTime);
	if (glfwGetKey(window, GLFW_KEY_X) == GLFW_PRESS)
		GCamera.ProcessKeyboard(kCameraDirectionDown, deltaTime);
}


void ProcessMouse(GLFWwindow* window, double xpos, double ypos)
{
	static float lastX = 0.0f;
	static float lastY = 0.0f;
	static bool firstMouse = true;
	if (firstMouse)
	{
		lastX = (float)xpos;
		lastY = (float)ypos;
		firstMouse = false;
	}

	float xoffset = (float)xpos - lastX;
	float yoffset = lastY - (float)ypos; // reversed since y-coordinates go from bottom to top

	lastX = (float)xpos;
	lastY = (float)ypos;

	GCamera.ProcessMouseMovement(xoffset, yoffset);
}


void UpdateRandomTexture(Texture2D& tex)
{
	static std::default_random_engine e;
	static std::uniform_real_distribution<> dis(-8.0, 8.0);

	const int kSize = 32 * 32 * 4;
	static float data[kSize];
	for(int i = 0; i < kSize; i++)
		data[i] = (float)dis(e);

	glBindTexture(GL_TEXTURE_2D, tex.GetTexture());
	glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, tex.GetWidth(), tex.GetHeight(), GL_RGBA, GL_FLOAT, data);
	CheckError("update");
	glBindTexture(GL_TEXTURE_2D, 0);
}


int main()
{
	srand(time(NULL));

	glfwSetErrorCallback(error_callback);
	glfwInit();

	glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 4);
	glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 5);
	glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
	glfwWindowHint(GLFW_OPENGL_DEBUG_CONTEXT, GLFW_TRUE);
	GLFWwindow* window = glfwCreateWindow(1024, 1024, "Sandbox", nullptr, nullptr);

	glfwSetCursorPosCallback(window, ProcessMouse);
	glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);

	glfwMakeContextCurrent(window);
	glfwSwapInterval(1);

	// start GLEW extension handler
	glewExperimental = GL_TRUE;
	glewInit();

	glDebugMessageCallback( myCallback, nullptr );

	int work_grp_cnt[3];

	glGetIntegeri_v(GL_MAX_COMPUTE_WORK_GROUP_COUNT, 0, &work_grp_cnt[0]);
	glGetIntegeri_v(GL_MAX_COMPUTE_WORK_GROUP_COUNT, 1, &work_grp_cnt[1]);
	glGetIntegeri_v(GL_MAX_COMPUTE_WORK_GROUP_COUNT, 2, &work_grp_cnt[2]);

	printf("max global (total) work group size x:%i y:%i z:%i\n", work_grp_cnt[0], work_grp_cnt[1], work_grp_cnt[2]);

	VertexShader vs("shaders/base.vert");
	FragmentShader fs("shaders/base.frag");
	Shader* shaders[] = {&vs, &fs};
	Program program(shaders, 2);
	if(!program.IsValid())
		return 1;

	Texture2D simulationTex(GFieldSizeX, GFieldSizeY, FORMAT_R32U);
	GLuint simulationFramebuffer;
	glGenFramebuffers(1, &simulationFramebuffer);
	glBindFramebuffer(GL_FRAMEBUFFER, simulationFramebuffer);
	glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, simulationTex.GetTexture(), 0);
	glBindFramebuffer(GL_FRAMEBUFFER, 0);
	if (!CheckError("Failed to create framebuffer"))
		return 1;

	Texture2D randomTex(32, 32, FORMAT_RGBA32F);

	if (!GBuildings.Init(GFieldSizeX, GFieldSizeY))
		return 1;

	if (!GUnits.Init(GUnitsNum, GFieldSizeX, GFieldSizeY))
		return false;

	GCamera.m_pos = glm::vec3(GFieldSizeX * 0.5f, 32.0f, GFieldSizeY * 0.5f);

	GLuint dummyVao;
	glGenVertexArrays(1, &dummyVao);

	static double lastFrame = glfwGetTime();
	double counter = 0.0f;

	while (!glfwWindowShouldClose(window))
	{
		double currentFrame = glfwGetTime();
		GDeltaTime = currentFrame - lastFrame;
		lastFrame = currentFrame;
		counter += GDeltaTime;
		if (counter > 1.0)
		{
			printf("Frame time: %fms\n", GDeltaTime * 1000.0);
			counter = 0.0;
		}

		ProcessInput(window);

		glBindFramebuffer(GL_DRAW_FRAMEBUFFER, simulationFramebuffer);
		glViewport(0, 0, GFieldSizeX, GFieldSizeY);
		glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
		glClear(GL_COLOR_BUFFER_BIT);

		glBindFramebuffer(GL_FRAMEBUFFER, 0);
		int width, height;
		glfwGetFramebufferSize(window, &width, &height);

		glViewport(0, 0, width, height);
		glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

		UpdateRandomTexture(randomTex);

		GUnits.Simulate(simulationTex, randomTex);

		/*glUseProgram(program.GetProgram());
		program.SetVec4("targetSize", glm::vec4(width, height, 0, 0));
		program.SetTexture("units", tex);
		//program.SetTexture("map", GBuildings_.GetMap());
		program.BindUniforms();
		glBindVertexArray(dummyVao);
		glDrawArrays(GL_TRIANGLES, 0, 6);*/

		glm::mat4 proj = glm::perspective(glm::radians(45.0f), (float)width / (float)height, 0.1f, 4000.0f);
		glm::mat4 view = GCamera.GetViewMatrix();
		glm::mat4 viewProj = proj * view;
		GBuildings.Draw(viewProj, GCamera.m_dir);

		GUnits.Draw(viewProj, glm::transpose(view));

		glfwSwapBuffers(window);
		glfwPollEvents();
	}

	glfwDestroyWindow(window);
	glfwTerminate();
}
