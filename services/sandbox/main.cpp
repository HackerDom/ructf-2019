#include <vector>
#include <math.h>
#include <time.h>
#include <random>
#include "glwrap.h"
#include "buildings.h"

static void error_callback(int error, const char* description)
{
	fprintf(stderr, "Error: %s\n", description);
}

void myCallback( GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, const GLchar *msg, const void *data )
{
	printf("GL Error: %s\n", msg);
}


enum kUnitType
{
	EUnitHuman = 0,
	EUnitSmith
};


struct Unit
{
	uint32_t mind[8];
	uint32_t id;
	float posX;
	float posY;
	uint32_t type;
	float power;
	uint32_t prevDirIdx;
	uint32_t prevCrossIdx;
	//float prevDir[2];
	float padding1;
};
static_assert (sizeof(Unit) == 16 * 4, "hey!");


struct Building
{
	float posX;
	float posY;
	float sizeX;
	float sizeY;
	float color[4];
};


std::vector<Unit> GUnits;
std::vector<Building> GBuildings;
Buildings GBuildings_;
Camera GCamera;
double GDeltaTime;
uint32_t GFieldSizeX = 4096 + 8;
uint32_t GFieldSizeY = 4096 + 8;


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


void GenerateUnits()
{
	std::default_random_engine e;
	std::uniform_real_distribution<> dis(0.1, 1.0);

	uint32_t num = 256 * 1024;
	for(uint32_t i = 0; i < num; i++)
	{
		Unit u;

		char mind[32];
		for(uint32_t j = 0; j < 32; j++)
			mind[j] = rand() % 256;
		memcpy(u.mind, mind, 32);

		u.id = rand();

		u.posX = 256.0f + 4.0f + (float)dis(e);
		u.posY = 256.0f + 4.0f + (float)dis(e);

		u.type = rand() % 2;
		u.power = (float)dis(e);
		u.prevDirIdx = 0;
		u.prevCrossIdx = 0;

		GUnits.push_back(u);
	}
}


void GenerateBuildings()
{
	std::default_random_engine e;
	std::uniform_real_distribution<> disColor(0.0, 0.8);

	const float floatStreetLength = kStreetWidth;

	float newBuildingLeft = floatStreetLength;
	float newBuildingTop = floatStreetLength;

	while (1)
	{
		Building b;

		b.sizeX = kBuildingSize;
		b.sizeY = kBuildingSize;
		b.color[0] = (float)disColor(e);
		b.color[1] = (float)disColor(e);
		b.color[2] = (float)disColor(e);
		b.color[3] = 0.0f;

		if (newBuildingLeft + b.sizeX > (float)GFieldSizeX)
		{
			newBuildingLeft = floatStreetLength;
			newBuildingTop += b.sizeY + floatStreetLength;
			if (newBuildingTop > (float)GFieldSizeY)
				break;
		}
		
		b.posX = newBuildingLeft + b.sizeX * 0.5f;
		b.posY = newBuildingTop + b.sizeY * 0.5f;

		newBuildingLeft += b.sizeX + floatStreetLength;

		GBuildings.push_back(b);
	}
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

	GenerateUnits();
	GenerateBuildings();

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

	ComputeShader cs("shaders/test.cs");
	Shader* csArray[] = {&cs};
	Program csProg(csArray, 1);
	if(!csProg.IsValid())
		return 1;

	ComputeShader clearCs("shaders/clear.cs");
	csArray[0] = {&clearCs};
	Program clearCsProg(csArray, 1);
	if(!clearCsProg.IsValid())
		return 1;

	ComputeShader buildingCs("shaders/buildings.cs");
	csArray[0] = {&buildingCs};
	Program buildingCsProg(csArray, 1);
	if (!buildingCsProg.IsValid())
		return 1;

	if (!GBuildings_.Init(GFieldSizeX, GFieldSizeY))
		return 1;

	GCamera.m_pos = glm::vec3(GFieldSizeX * 0.5f, 32.0f, GFieldSizeY * 0.5f);

	GLuint dummyVao;
	glGenVertexArrays(1, &dummyVao);

	Texture2D tex(GFieldSizeX, GFieldSizeY, FORMAT_RGBA16F);
	Texture2D map(GFieldSizeX, GFieldSizeY, FORMAT_RGBA16F);
	Texture2D randomTex(32, 32, FORMAT_RGBA32F);

	GLuint unitsSsbo;
	glGenBuffers(1, &unitsSsbo);
	glBindBuffer(GL_SHADER_STORAGE_BUFFER, unitsSsbo);
	glBufferData(GL_SHADER_STORAGE_BUFFER, GUnits.size() * sizeof(Unit), GUnits.data(), GL_DYNAMIC_DRAW);
	glBindBuffer(GL_SHADER_STORAGE_BUFFER, 0); // unbind

	GLuint buildingsSsbo;
	glGenBuffers(1, &buildingsSsbo);
	glBindBuffer(GL_SHADER_STORAGE_BUFFER, buildingsSsbo);
	glBufferData(GL_SHADER_STORAGE_BUFFER, GBuildings.size() * sizeof(Building), GBuildings.data(), GL_DYNAMIC_DRAW);
	glBindBuffer(GL_SHADER_STORAGE_BUFFER, 0); // unbind

	// fill map
	{
		glUseProgram(clearCsProg.GetProgram());
		clearCsProg.SetImage("img_output", map, GL_WRITE_ONLY);
		clearCsProg.BindUniforms();
		glDispatchCompute(GFieldSizeX / 8, GFieldSizeY / 8, 1);
		glMemoryBarrier(GL_SHADER_IMAGE_ACCESS_BARRIER_BIT);

		glUseProgram(buildingCsProg.GetProgram());
		buildingCsProg.SetImage("img_output", map, GL_WRITE_ONLY);
		buildingCsProg.SetSSBO("Buildings", buildingsSsbo);
		buildingCsProg.SetIVec4("buildingsCount", glm::ivec4(GBuildings.size(), 0, 0, 0));
		buildingCsProg.BindUniforms();
		glDispatchCompute((GBuildings.size() + 31) / 32, 1, 1);
		glMemoryBarrier(GL_SHADER_IMAGE_ACCESS_BARRIER_BIT);
	}

	static double lastFrame = glfwGetTime();

	while (!glfwWindowShouldClose(window))
	{
		double currentFrame = glfwGetTime();
		GDeltaTime = currentFrame - lastFrame;
		lastFrame = currentFrame;

		ProcessInput(window);

		int width, height;
		glfwGetFramebufferSize(window, &width, &height);

		glViewport(0, 0, width, height);
		glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

		glUseProgram(clearCsProg.GetProgram());
		clearCsProg.SetImage("img_output", tex, GL_WRITE_ONLY);
		clearCsProg.BindUniforms();
		glDispatchCompute(GFieldSizeX / 8, GFieldSizeY / 8, 1);
		glMemoryBarrier(GL_SHADER_IMAGE_ACCESS_BARRIER_BIT);

		UpdateRandomTexture(randomTex);

		glUseProgram(csProg.GetProgram());
		csProg.SetImage("img_output", tex, GL_WRITE_ONLY);
		csProg.SetIVec4("unitsCount", glm::ivec4(GUnits.size(), 0, 0, 0));
		csProg.SetTexture("randomTex", randomTex);
		csProg.SetImage("mapImage", map, GL_READ_ONLY);
		csProg.SetSSBO("Units", unitsSsbo);
		csProg.BindUniforms();
		glDispatchCompute((GUnits.size() + 31) / 32, 1, 1);

		glMemoryBarrier(GL_SHADER_IMAGE_ACCESS_BARRIER_BIT | GL_SHADER_STORAGE_BARRIER_BIT);

		/*glUseProgram(program.GetProgram());
		program.SetVec4("targetSize", glm::vec4(width, height, 0, 0));
		program.SetTexture("units", tex);
		program.SetTexture("map", map);
		program.BindUniforms();
		glBindVertexArray(dummyVao);
		glDrawArrays(GL_TRIANGLES, 0, 6);*/

		glm::mat4 proj = glm::perspective(glm::radians(45.0f), (float)width / (float)height, 0.1f, 4000.0f);
		glm::mat4 view = GCamera.GetViewMatrix();
		glm::mat4 viewProj = proj * view;
		GBuildings_.Draw(viewProj, GCamera.m_dir);

		glfwSwapBuffers(window);
		glfwPollEvents();
	}

	glfwDestroyWindow(window);
	glfwTerminate();
}
