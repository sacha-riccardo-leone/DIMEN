#version 330
in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 vertexTexCoord;

out vec3 fragPosition;
out vec3 fragNormal;
out vec2 fragTexCoord;

uniform mat4 mvp;

void main() {
    fragPosition = vertexPosition;
    fragNormal = vertexNormal;
    fragTexCoord = vertexTexCoord;
    gl_Position = mvp * vec4(vertexPosition, 1.0);
}
