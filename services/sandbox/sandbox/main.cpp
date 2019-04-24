#include <vector>
#include <math.h>
#include <time.h>
#include <random>
#include <glm/gtc/matrix_transform.hpp>
#include "glwrap.h"
#include "camera.h"
#include "buildings.h"
#include "units.h"
#include "interface.h"
#include "thread_affinity.h"
#include "gpu_camera.h"


static void GlfwErrorCallback(int error, const char* description)
{
	fprintf(stderr, "GLFW Error: %s\n", description);
}


void GlDebugCallback(GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, const GLchar *msg, const void *data)
{
	if (id == 131185 || id == 131186)
		return;

	const char* debSource = "";
	const char* debType = "";
	const char* debSev = "";

	if (source == GL_DEBUG_SOURCE_API)
		debSource = "OpenGL";
	else if (source == GL_DEBUG_SOURCE_WINDOW_SYSTEM)
		debSource = "Windows";
	else if (source == GL_DEBUG_SOURCE_SHADER_COMPILER)
		debSource = "Shader Compiler";
	else if (source == GL_DEBUG_SOURCE_THIRD_PARTY)
		debSource = "Third Party";
	else if (source == GL_DEBUG_SOURCE_APPLICATION)
		debSource = "Application";
	else if (source == GL_DEBUG_SOURCE_OTHER)
		debSource = "Other";

	if (type == GL_DEBUG_TYPE_ERROR)
		debType = "Error";
	else if (type == GL_DEBUG_TYPE_DEPRECATED_BEHAVIOR)
		debType = "Deprecated behavior";
	else if (type == GL_DEBUG_TYPE_UNDEFINED_BEHAVIOR)
		debType = "Undefined behavior";
	else if (type == GL_DEBUG_TYPE_PORTABILITY)
		debType = "Portability";
	else if (type == GL_DEBUG_TYPE_PERFORMANCE)
		debType = "Performance";
	else if (type == GL_DEBUG_TYPE_OTHER)
		debType = "Other";

	bool showMes = true;
	if (severity == GL_DEBUG_SEVERITY_HIGH)
		debSev = "High";
	else if (severity == GL_DEBUG_SEVERITY_MEDIUM)
		debSev = "Medium";
	else if (severity == GL_DEBUG_SEVERITY_LOW)
		debSev = "Low";
	else
		showMes = false;

	printf("OpenGL debug:\n\tSource:%s\n\tType:%s\n\tID:%u\n\tSeverity:%s\n\tMessage:%s\n", debSource, debType, id, debSev, msg);
}


static Buildings GBuildings;
static Units GUnits;
static Camera GCamera;
static GpuCamera GGpuCamera;
static double GDeltaTime;
static uint32_t GFieldSizeX = 4096 + kStreetWidth;
static uint32_t GFieldSizeY = 4096 + kStreetWidth;
#if DEBUG
static bool GSpectatorMode = false;
#else
static bool GSpectatorMode = true;
#endif


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

#if DEBUG
	if (glfwGetKey(window, GLFW_KEY_Q) == GLFW_PRESS)
		GSpectatorMode = !GSpectatorMode;
#endif
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
	for (int i = 0; i < kSize; i++)
		data[i] = (float)dis(e);

	glBindTexture(GL_TEXTURE_2D, tex.GetTexture());
	glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, tex.GetWidth(), tex.GetHeight(), GL_RGBA, GL_FLOAT, data);
	CheckError("update");
	glBindTexture(GL_TEXTURE_2D, 0);
}


void InterfaceCallback(const CommandHeader& cmd, char* data, char*& response, uint32_t& responseSize)
{
	static char uuidStr[64] = {};
	static char responseBuffer[512];
	response = responseBuffer;
	responseSize = 0;

	UUID uuid;
	memcpy(uuid.data(), cmd.uuid, 16);

	switch (cmd.cmd)
	{
		case kCommandAddUnit:
		{
			CommandAddUnit* addUnit = (CommandAddUnit*)data;
			CommandAddUnitResponse addUnitResponse;
			auto result = GUnits.AddUnit(uuid, addUnit->mind);
			addUnitResponse.result = (CommandAddUnitResponse::EResult)result;
			memcpy(response, &addUnitResponse, sizeof(addUnitResponse));
			responseSize = sizeof(addUnitResponse);

			const char* resultStr = "Unknown";
			if (result == Units::kAddOk)
				resultStr = "Ok";
			else if (result == Units::kAddTooMuchUnits)
				resultStr = "Error, too much units";
			if (result == Units::kAddAlreadyExists)
				resultStr = "Error, unit already exists";

			uuid_unparse(uuid, uuidStr);
			printf("Add unit %s result: %s\n", uuidStr, resultStr);
		}
		break;
		case kCommandGetUnit:
		{
			CommandGetUnit* getUnit = (CommandGetUnit*)data;
			CommandGetUnitResponse getUnitResponse;
			const Unit* unit = GUnits.GetUnit(uuid);
			const char* resultStr = "Ok";
			if (unit)
			{
				getUnitResponse.ok = true;
				memcpy(getUnitResponse.mind, unit->mind, 32);
				getUnitResponse.posX = unit->posX;
				getUnitResponse.posY = unit->posY;
				getUnitResponse.posZ = unit->posZ;
				getUnitResponse.power = unit->power;
			}
			else
			{
				getUnitResponse.ok = false;
				resultStr = "Not found";
			}
			memcpy(response, &getUnitResponse, sizeof(getUnitResponse));
			responseSize = sizeof(getUnitResponse);

			uuid_unparse(uuid, uuidStr);
			printf("Get unit %s: %s\n", uuidStr, resultStr);
		}
		break;

		default:
			printf("Unknown command\n");
			break;
	}
}


int main()
{
#if LINUX
	setenv("DISPLAY", ":0", 0);
#endif
	srand(time(NULL));

	if (PinThreadToCore(0) != 0)
	{
		perror("PinThreadToCore");
		return 1;
	}

	InitInterface(InterfaceCallback);

	glfwSetErrorCallback(GlfwErrorCallback);
	glfwInit();

	glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 4);
	glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 5);
	glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
#if DEBUG
	glfwWindowHint(GLFW_OPENGL_DEBUG_CONTEXT, GLFW_TRUE);
	GLFWwindow* window = glfwCreateWindow(1024, 1024, "Sandbox", nullptr, nullptr);
#else
	GLFWwindow* window = glfwCreateWindow(1920, 1080, "Sandbox", glfwGetPrimaryMonitor(), nullptr);
#endif

#if DEBUG
	glfwSetCursorPosCallback(window, ProcessMouse);
#endif
	glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);

	glfwMakeContextCurrent(window);
	glfwSwapInterval(1);

	// start GLEW extension handler
	glewExperimental = GL_TRUE;
	glewInit();

	glDebugMessageCallback(GlDebugCallback, nullptr);
	glEnable(GL_DEBUG_OUTPUT_SYNCHRONOUS);

	printf("GL Vendor: \"%s\"\n", glGetString(GL_VENDOR));
	printf("GL Renderer: \"%s\"\n", glGetString(GL_RENDERER));
	printf("GL Version: \"%s\"\n", glGetString(GL_VERSION));

	Texture2D randomTex(32, 32, FORMAT_RGBA32F);

	if (!GBuildings.Init(GFieldSizeX, GFieldSizeY))
		return 1;

	if (!GUnits.Init(GFieldSizeX, GFieldSizeY))
		return false;

	GCamera.m_pos = glm::vec3(GFieldSizeX * 0.5f, 32.0f, GFieldSizeY * 0.5f);

	if (!GGpuCamera.Init(GCamera.GetViewMatrix(), GCamera.m_pos, GCamera.m_dir, GCamera.m_up))
		return false;
	GGpuCamera.EnableForceMode(!GSpectatorMode);

	GLuint dummyVao;
	glGenVertexArrays(1, &dummyVao);

	static double lastFrame = glfwGetTime();
	double counter = 0.0f;
	double followTime = 0.0f;
	std::default_random_engine randomEngine;

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

#if DEBUG
		ProcessInput(window);
#endif

		int width, height;
		glfwGetFramebufferSize(window, &width, &height);

		glViewport(0, 0, width, height);
		glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

		UpdateRandomTexture(randomTex);
		GUnits.Simulate(randomTex);

		const float fovY = glm::radians(45.0f);
		float aspect = (float)width / (float)height;
		float near = 0.1f;
		float far = 4000.0f;
		glm::mat4 proj = glm::perspective(fovY, aspect, near, far);

		if (GGpuCamera.IsForceModeEnabled() != !GSpectatorMode)
			GGpuCamera.EnableForceMode(!GSpectatorMode);

		if (!GSpectatorMode)
			GGpuCamera.ForceCameraData(GCamera.m_pos, GCamera.m_dir, GCamera.m_up);

		uint32_t unitsNumber = GUnits.GetUnitsNumber();
		if (unitsNumber)
		{
			if (GGpuCamera.GetUnitIdxToFollow() == -1)
			{
				GGpuCamera.SetUnitToFollow(0);
				followTime = 0.0f;
			}

			if (followTime > 20.0f)
			{
				std::uniform_real_distribution<> dis(0, unitsNumber - 1);
				int idx = dis(randomEngine);
				GGpuCamera.SetUnitToFollow(idx);
				followTime = 0.0f;
			}
			followTime += GDeltaTime;
		}

		GGpuCamera.Update(fovY, aspect, near, far, GUnits.GetSSBO());
		GBuildings.Draw(proj, GGpuCamera.GetDataSSBO());
		GUnits.Draw(proj, GGpuCamera.GetDataSSBO());

		glfwSwapBuffers(window);
		glfwPollEvents();
	}

	glfwDestroyWindow(window);
	glfwTerminate();
	ShutdownInterface();
	GUnits.Shutdown();

	printf("Done\n");
}
