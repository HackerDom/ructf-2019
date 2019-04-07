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
	int32_t x;
	int32_t y;
	int32_t z;
	int32_t w;	
};