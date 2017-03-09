
layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec2 in_texcoord;

uniform mat4 model_matrix, view_matrix, projection_matrix;

out vec3 world_pos;
out vec3 world_normal;
out vec2 texcoord;
out vec4 viewSpace;

void main() {

	//used for lighting models
	world_pos = (model_matrix * vec4(in_position, 1)).xyz;
	world_normal = normalize(mat3(model_matrix) * in_normal);
	texcoord = in_texcoord;

	//send it to fragment shader
	viewSpace = view_matrix * model_matrix * vec4(in_position, 1);
	gl_Position = projection_matrix * viewSpace;

}

layout(location = 0) out vec4 out_color;

uniform vec3 light_position;
uniform vec3 eye_position;

uniform sampler2D texture1;


uniform int fogSelector;

uniform int depthFog;

const vec3 DiffuseLight = vec3(0.15, 0.05, 0.0);
const vec3 RimColor = vec3(0.2, 0.2, 0.2);


in vec3 world_pos;
in vec3 world_normal;
in vec4 viewSpace;
in vec2 texcoord;

const vec3 fogColor = vec3(0.5, 0.5, 0.5);
const float FogDensity = 0.05;

void main() {

	vec3 tex1 = texture(texture1, texcoord).rgb;

	vec3 L = normalize(light_position - world_pos);
	vec3 V = normalize(eye_position - world_pos);
	vec3 diffuse = DiffuseLight * max(0, dot(L, world_normal));

	float rim = 1 - max(dot(V, world_normal), 0.0);
	rim = smoothstep(0.6, 1.0, rim);
	vec3 finalRim = RimColor * vec3(rim, rim, rim);
	vec3 lightColor = finalRim + diffuse + tex1;

	vec3 finalColor = vec3(0, 0, 0);

	float dist = 0;
	float fogFactor = 0;

	if (depthFog == 0)
	{
		dist = abs(viewSpace.z);
	}
	else
	{
		dist = length(viewSpace);
	}

	if (fogSelector == 0)
	{
		fogFactor = (80 - dist) / (80 - 20);
		fogFactor = clamp(fogFactor, 0.0, 1.0);

		finalColor = mix(fogColor, lightColor, fogFactor);
	}
	else if (fogSelector == 1)
	{
		fogFactor = 1.0 / exp(dist * FogDensity);
		fogFactor = clamp(fogFactor, 0.0, 1.0);

		finalColor = mix(fogColor, lightColor, fogFactor);
	}
	else if (fogSelector == 2)
	{
		fogFactor = 1.0 / exp((dist * FogDensity)* (dist * FogDensity));
		fogFactor = clamp(fogFactor, 0.0, 1.0);

		finalColor = mix(fogColor, lightColor, fogFactor);
	}

	out_color = vec4(finalColor, 1);

}

