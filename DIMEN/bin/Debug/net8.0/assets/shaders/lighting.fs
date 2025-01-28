#version 330
in vec3 fragPosition;
in vec3 fragNormal;
in vec2 fragTexCoord;

out vec4 finalColor;

uniform vec3 lightPosition;
uniform sampler2D texture0; // Albedo

void main() {
    vec3 lightDir = normalize(lightPosition - fragPosition);
    float diff = max(dot(fragNormal, lightDir), 0.0);
    vec4 texColor = texture(texture0, fragTexCoord);
    finalColor = vec4(texColor.rgb * diff, texColor.a);
}