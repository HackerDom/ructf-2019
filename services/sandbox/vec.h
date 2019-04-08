#pragma once


//
struct Vec4
{
	float x;
	float y;
	float z;
	float w;	

	Vec4()
		: x( 0.0f ), y( 0.0f ), z( 0.0f ), w( 0.0f )
	{}
	
	Vec4( float x_, float y_, float z_, float w_ )
		: x( x_ ), y( y_ ), z( z_ ), w( w_ )
	{}
};


//
struct IVec4
{
	int32_t x = 0;
	int32_t y = 0;
	int32_t z = 0;
	int32_t w = 0;

	IVec4() = default;

	IVec4( int32_t x_, int32_t y_, int32_t z_, int32_t w_ )
		: x( x_ ), y( y_ ), z( z_ ), w( w_ )
	{}
};
