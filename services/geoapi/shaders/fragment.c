
uniform highp vec4 viewPortSize;
uniform highp vec3 uniformArrayOfStructs[3];

void main() {
    gl_FragColor.x = 1.0;

    highp vec4 pos = vec4(gl_FragCoord.x, gl_FragCoord.y, 0.0, 1.0)/viewPortSize;

    highp float tileY = floor((gl_FragCoord.y)/4.0);
    highp float tileX = floor((gl_FragCoord.x)/4.0);

    gl_FragColor = vec4(0.0, 0.0, 0.0, 0.0);

    if(mod(tileX, 2.0)  == 0.0)
   	  gl_FragColor.x = uniformArrayOfStructs[1].x;
    else if(mod(tileY, 2.0)  == 0.0)
   	  gl_FragColor.y = 1.0;

    gl_FragColor.x = 1.0;
}
