#pragma once
#include <map>


class NonCopyable
{
public:
	NonCopyable() = default;
	NonCopyable(const NonCopyable&) = delete;
	NonCopyable(const NonCopyable&&) = delete;
	NonCopyable& operator=(const NonCopyable&) = delete;
};


class Shader : public NonCopyable
{
public:
	Shader() = delete;
	Shader(GLuint type, const char* fileName);
	~Shader();

	GLuint GetShader() const;
	bool IsValid() const;

private:
	GLuint m_shader = 0;
};


class VertexShader : public Shader
{
public:
	VertexShader() = delete;
	VertexShader(const char* fileName);
};


class FragmentShader : public Shader
{
public:
	FragmentShader() = delete;
	FragmentShader(const char* fileName);
};


class ComputeShader : public Shader
{
public:
	ComputeShader() = delete;
	ComputeShader(const char* fileName);
};


class Program : public NonCopyable
{
public:
	Program() = delete;
	Program(Shader** shaders, uint32_t shadersNum);
	~Program();

	bool IsValid() const;
	GLuint GetProgram() const;

	bool SetTexture(const char* uniformName, const Texture2D& tex);
	bool SetImage(const char* uniformName, const Texture2D& tex, GLenum access);
	bool SetSSBO(const char* uniformName, GLuint ssbo);
	bool SetVec4(const char* uniformName, const Vec4& v);
	bool SetIVec4(const char* uniformName, const IVec4& v);
	bool SetAttribute(const char* attrName,
	                  GLint size,
	                  GLenum type,
	                  GLboolean normalized,
	                  GLsizei stride,
	                  void* data,
	                  int dataSizeInBytes);
	void BindUniforms() const;

private:

	struct VertexAttribute
	{
		GLint size = 0;
		GLenum type = 0;
		GLboolean normalized = 0;
		GLsizei stride = 0;
		GLuint buffer = 0;
		bool valid = false;
	};

	struct ImageBind
	{
		GLuint texture;
		GLenum access;
		GLenum format;
	};

	GLuint m_program = 0;
	std::map<GLint, VertexAttribute> m_attributes;
	std::map<GLint, GLuint> m_texBinds;
	std::map<GLint, ImageBind> m_imageBinds;
	std::map<GLint, GLuint> m_ssboBinds;
	std::map<GLint, Vec4> m_vec4s;
	std::map<GLint, IVec4> m_ivec4s;
};
