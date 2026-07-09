#version 450
// Fullscreen triangle — no vertex buffers. gl_VertexIndex 0,1,2 spans the screen with a
// single oversized triangle; vUV runs 0..1 across the visible quad.
layout(location = 0) out vec2 vUV;
void main()
{
    vec2 p = vec2(float((gl_VertexIndex << 1) & 2), float(gl_VertexIndex & 2));
    vUV = p;
    gl_Position = vec4(p * 2.0 - 1.0, 0.0, 1.0);
}
