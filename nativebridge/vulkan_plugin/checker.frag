#version 450
// Rotating checkerboard. The push-constant angle spins the sampling grid about the centre,
// so the whole board appears to rotate. gridN = number of squares across.
layout(location = 0) in  vec2 vUV;
layout(location = 0) out vec4 outColor;

layout(push_constant) uniform PC {
    float angle;   // radians, advanced each frame on the C++ side
    float gridN;   // squares across the board
} pc;

void main()
{
    vec2 uv = vUV - 0.5;                          // centre the rotation
    float c = cos(pc.angle), s = sin(pc.angle);
    vec2 r = vec2(uv.x * c - uv.y * s,
                  uv.x * s + uv.y * c) + 0.5;
    vec2 g = floor(r * pc.gridN);
    float chk = mod(g.x + g.y, 2.0);
    vec3 col = mix(vec3(0.05, 0.05, 0.08),        // dark squares
                   vec3(0.95, 0.70, 0.20),        // amber squares
                   chk);
    outColor = vec4(col, 1.0);
}
