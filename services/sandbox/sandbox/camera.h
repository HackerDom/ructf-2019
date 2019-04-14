#pragma once

enum ECameraDirection
{
	kCameraDirectionForward,
	kCameraDirectionBackward,
	kCameraDirectionLeft,
	kCameraDirectionRight,
	kCameraDirectionUp,
	kCameraDirectionDown
};


static const float kSpeed = 25.0f;
static const float kSensitivity = 0.1f;


class Camera
{
public:
	glm::vec3 m_pos;
	glm::vec3 m_dir;
	glm::vec3 m_up;
	glm::vec3 m_right;
	float m_yaw;
	float m_pitch;

	Camera(glm::vec3 position = glm::vec3(0.0f, 0.0f, 0.0f), float yaw = -90.0f, float pitch = 0.0f) 
		: m_dir(glm::vec3(0.0f, 0.0f, -1.0f))
	{
		m_pos = position;
		m_yaw = yaw;
		m_pitch = pitch;
		updateCameraVectors();
	}

	glm::mat4 GetViewMatrix()
	{
		return glm::lookAt(m_pos, m_pos + m_dir, m_up);
	}

	void ProcessKeyboard(ECameraDirection direction, float deltaTime)
	{
		float velocity = kSpeed * deltaTime;
		if (direction == kCameraDirectionForward)
			m_pos += m_dir * velocity;
		if (direction == kCameraDirectionBackward)
			m_pos -= m_dir * velocity;
		if (direction == kCameraDirectionLeft)
			m_pos -= m_right * velocity;
		if (direction == kCameraDirectionRight)
			m_pos += m_right * velocity;
		if (direction == kCameraDirectionUp)
			m_pos += m_up * velocity;
		if (direction == kCameraDirectionDown)
			m_pos -= m_up * velocity;
	}

	void ProcessMouseMovement(float xoffset, float yoffset, GLboolean constrainPitch = true)
	{
		xoffset *= kSensitivity;
		yoffset *= kSensitivity;

		m_yaw += xoffset;
		m_pitch += yoffset;

		if (constrainPitch)
		{
			if (m_pitch > 89.0f)
				m_pitch = 89.0f;
			if (m_pitch < -89.0f)
				m_pitch = -89.0f;
		}

		updateCameraVectors();
	}

private:
	void updateCameraVectors()
	{
		glm::vec3 front;
		front.x = cos(glm::radians(m_yaw)) * cos(glm::radians(m_pitch));
		front.y = sin(glm::radians(m_pitch));
		front.z = sin(glm::radians(m_yaw)) * cos(glm::radians(m_pitch));
		m_dir = glm::normalize(front);
		m_right = glm::normalize(glm::cross(m_dir, glm::vec3(0.0f, 1.0f, 0.0f)));
		m_up = glm::normalize(glm::cross(m_right, m_dir));
	}
};


struct FrustumPlanes
{
	enum
	{
		kTop = 0,
		kBottom,
		kLeft,
		kRight,
		kFar,
		kNear,

		kPlanesCount
	};
};


inline void BuildFrustumPlane(glm::vec4& plane, const glm::vec3& n, const glm::mat4& viewMatrix, const glm::vec3& org, float offset)
{
	glm::vec3 xn = glm::normalize(n * glm::mat3(viewMatrix));
	plane = glm::vec4(xn.x, xn.y, xn.z, glm::dot(org, xn) + offset);
}


inline void BuildFrustumPlane(glm::vec4& plane, const glm::vec3 v[3], const glm::mat4& viewMatrix, const glm::vec3& org, float offset)
{
	glm::vec3 n = -glm::cross((v[0] - v[1]), (v[2] - v[1]));
	BuildFrustumPlane(plane, n, viewMatrix, org, offset);
}


inline void BuildFrustumPlanes(const glm::vec3& origin, const glm::mat4& viewMatrix, float fovy, float aspect, float znear, float zfar, glm::vec4 planes[])
{
	glm::vec3 v[3];
	float rx, ry;

	float tanFovY = tanf(fovy * 0.5f);
	float tanFovX = tanFovY * aspect;
	
	rx = tanFovX * znear;
	ry = tanFovY * znear;

	v[0] = glm::vec3(0.0f, 0.0f, 0.0f);

	// top (+)
	v[2] = glm::vec3(-rx, +ry, znear);
	v[1] = glm::vec3(+rx, +ry, znear);
	BuildFrustumPlane(planes[FrustumPlanes::kTop], v, viewMatrix, origin, 0);

	// bottom (+)
	v[1] = glm::vec3(-rx, -ry, znear);
	v[2] = glm::vec3(+rx, -ry, znear);
	BuildFrustumPlane(planes[FrustumPlanes::kBottom], v, viewMatrix, origin, 0);

	// left (+)
	v[2] = glm::vec3(-rx, -ry, znear);
	v[1] = glm::vec3(-rx, +ry, znear);
	BuildFrustumPlane(planes[FrustumPlanes::kLeft], v, viewMatrix, origin, 0);

	// right (+)
	v[1] = glm::vec3(+rx, -ry, znear);
	v[2] = glm::vec3(+rx, +ry, znear);
	BuildFrustumPlane(planes[FrustumPlanes::kRight], v, viewMatrix, origin, 0);

	// farthest (+)
	v[0] = glm::vec3(0.0f, 0.0f, -1.0f);
	BuildFrustumPlane(planes[FrustumPlanes::kFar], v[0], viewMatrix, origin, zfar);

	// back (+)
	v[0] = glm::vec3(0.0f, 0.0f, +1.0f);
	BuildFrustumPlane(planes[FrustumPlanes::kNear], v[0], viewMatrix, origin, -znear);
}