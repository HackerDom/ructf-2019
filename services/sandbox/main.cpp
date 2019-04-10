#include <vector>
#include <math.h>
#include <time.h>
#include <random>
#include "glwrap.h"

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
};

std::vector<Unit> GUnits;
uint32_t GFieldSizeX = 1024;
uint32_t GFieldSizeY = 1024;

void GenerateUnits()
{
	uint32_t num = 256;
	for(uint32_t i = 0; i < num; i++)
	{
		Unit u;

		char mind[32];
		for(uint32_t j = 0; j < 32; j++)
			mind[j] = rand() % 256;
		memcpy(u.mind, mind, 32);

		u.id = rand();

		u.posX = (float)(rand() % GFieldSizeX);
		u.posY = (float)(rand() % GFieldSizeY);

		u.type = rand() % 2;
		u.power = 1.0f;

		GUnits.push_back(u);
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

	glfwSetErrorCallback(error_callback);
	glfwInit();

	glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 4);
	glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 5);
	glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
	glfwWindowHint(GLFW_OPENGL_DEBUG_CONTEXT, GLFW_TRUE);
	GLFWwindow* window = glfwCreateWindow(768, 768, "Sandbox", nullptr, nullptr);

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

	VertexShader vs("base.vert");
	FragmentShader fs("base.frag");
	Shader* shaders[] = {&vs, &fs};
	Program program(shaders, 2);
	if(!program.IsValid())
		return 1;

	ComputeShader cs("test.cs");
	Shader* csArray[] = {&cs};
	Program csProg(csArray, 1);
	if(!csProg.IsValid())
		return 1;

	ComputeShader clearCs("clear.cs");
	csArray[0] = {&clearCs};
	Program clearCsProg(csArray, 1);
	if(!clearCsProg.IsValid())
		return 1;

	GLuint dummyVao;
	glGenVertexArrays(1, &dummyVao);

	Texture2D tex(GFieldSizeX, GFieldSizeY, FORMAT_RGBA16F);
	Texture2D randomTex(32, 32, FORMAT_RGBA32F);

	GLuint ssbo;
	glGenBuffers(1, &ssbo);
	glBindBuffer(GL_SHADER_STORAGE_BUFFER, ssbo);
	glBufferData(GL_SHADER_STORAGE_BUFFER, GUnits.size() * sizeof(Unit), GUnits.data(), GL_DYNAMIC_DRAW);
	glBindBuffer(GL_SHADER_STORAGE_BUFFER, 0); // unbind

	while (!glfwWindowShouldClose(window))
	{
		int width, height;
		glfwGetFramebufferSize(window, &width, &height);

		glViewport(0, 0, width, height);
		glClearColor(1.0f, 0.0f, 0.0f, 0.0f);
		glClear(GL_COLOR_BUFFER_BIT);

		glUseProgram(clearCsProg.GetProgram());
		clearCsProg.SetImage("img_output", tex, GL_WRITE_ONLY);
		clearCsProg.BindUniforms();
		glDispatchCompute(GFieldSizeX / 8, GFieldSizeY / 8, 1);
		glMemoryBarrier(GL_SHADER_IMAGE_ACCESS_BARRIER_BIT);

		UpdateRandomTexture(randomTex);

		glUseProgram(csProg.GetProgram());
		csProg.SetImage("img_output", tex, GL_WRITE_ONLY);
		csProg.SetIVec4("unitsCount", IVec4(GUnits.size(), 0, 0, 0));
		csProg.SetTexture("randomTex", randomTex);
		csProg.SetSSBO("Units", ssbo);
		csProg.BindUniforms();
		glDispatchCompute((GUnits.size() + 31) / 32, 1, 1);

		glMemoryBarrier(GL_SHADER_IMAGE_ACCESS_BARRIER_BIT | GL_SHADER_STORAGE_BARRIER_BIT);

		glUseProgram(program.GetProgram());
		program.SetVec4("targetSize", Vec4(width, height, 0, 0));
		program.SetTexture("img_input", tex);
		program.BindUniforms();
		glBindVertexArray(dummyVao);
		glDrawArrays(GL_TRIANGLES, 0, 6);

		glfwSwapBuffers(window);
		glfwPollEvents();
	}

	glfwDestroyWindow(window);
	glfwTerminate();
}
