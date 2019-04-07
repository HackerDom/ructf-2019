#pragma once
#include <map>


class Shader
{
public:
	Shader() = delete;
	Shader(GLuint type, const char* fileName);
	Shader(const Shader&) = delete;
	Shader(const Shader&&) = delete;
	Shader& operator=(const Shader&) = delete;
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
	VertexShader(const VertexShader&) = delete;
	VertexShader(const VertexShader&&) = delete;
	VertexShader& operator=(const VertexShader&) = delete;
};


class FragmentShader : public Shader
{
public:
	FragmentShader() = delete;
	FragmentShader(const char* fileName);
	FragmentShader(const FragmentShader&) = delete;
	FragmentShader(const FragmentShader&&) = delete;
	FragmentShader& operator=(const FragmentShader&) = delete;
};


class Program
{
public:
	Program() = delete;
	Program(const VertexShader& vs, const FragmentShader& fs);
	Program(const Program&) = delete;
	Program(const Program&&) = delete;
	Program& operator=(const Program&) = delete;
	~Program();

	bool IsValid() const;
	GLuint GetProgram() const;

	bool SetTexture(const char* uniformName, const Texture2D& tex);
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

	const VertexShader& m_vs;
	const FragmentShader& m_fs;
	GLuint m_program = 0;
	std::map<GLint, VertexAttribute> m_attributes;
	std::map<GLenum, GLuint> m_texBinds;
	std::map<GLenum, Vec4> m_vec4s;
	std::map<GLenum, IVec4> m_ivec4s;
};
