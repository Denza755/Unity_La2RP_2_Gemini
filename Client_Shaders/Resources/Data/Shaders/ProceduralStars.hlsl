
#ifndef PROCEDURAL_STARS_INCLUDED  
#define PROCEDURAL_STARS_INCLUDED

float3 ProceduralStars_Hash33(float3 p)  
{  
p = frac(p * float3(443.897f, 441.423f, 437.195f));  
p += dot(p, p.yxz + 19.19f);  
return frac((p.xxy + p.yxx) * p.zyx);  
}

float ProceduralStars_Hash13(float3 p)  
{  
p = frac(p * 0.1031f);  
p += dot(p, p.zyx + 31.32f);  
return frac((p.x + p.y) * p.z);  
}

float3 ProceduralStars_SpectralColor(float h)  
{  
// Более насыщенные и живые звездные оттенки (от голубого Сириуса до золотого Арктура и оранжевого Антареса)  
if (h < 0.18f)  
return lerp(float3(0.50f, 0.72f, 1.00f), float3(0.78f, 0.88f, 1.00f), h / 0.18f);  
else if (h < 0.55f)  
return float3(0.96f, 0.98f, 1.00f);  
else if (h < 0.82f)  
return lerp(float3(1.00f, 0.95f, 0.80f), float3(1.00f, 0.84f, 0.58f), (h - 0.55f) / 0.27f);  
else  
return lerp(float3(1.00f, 0.72f, 0.45f), float3(1.00f, 0.55f, 0.38f), (h - 0.82f) / 0.18f);  
}

void ProceduralStars_EvaluateLayer(  
float3 rayDir,  
float gridScale,  
float densityThreshold,  
float minRadius,  
float maxRadius,  
float baseHDR,  
bool hasHalo,  
float time,  
inout float3 accumColor,  
inout float accumAlpha)  
{  
float3 scaledPos = rayDir * gridScale;  
float3 cell = floor(scaledPos);  
float3 f = frac(scaledPos) - 0.5f;

// 1. Отсечение большинства пустых ячеек, чтобы небо было чистым и реалистичным  
float rarity = ProceduralStars_Hash13(cell);  
if (rarity < densityThreshold)  
return;

float3 hash = ProceduralStars_Hash33(cell + 17.3f);  
float3 hash2 = ProceduralStars_Hash33(cell + 53.7f);

// 2. Умеренное смещение от центра (макс +-0.26), чтобы ореол НИКОГДА не резался стенками кубика в квадратики!  
float3 offset = (hash - 0.5f) * 0.52f;  
float3 delta = f - offset;  
float dist = length(delta);

// Степенное распределение размера: много мелких точек и единицы крупных  
float sizeClass = pow(hash2.x, 2.6f);  
float radius = lerp(minRadius, maxRadius, sizeClass);

if (dist < radius)  
{  
float normDist = saturate(dist / radius);

// Острое точечное ядро без мыльного плоского круга  
float core = exp(-normDist * normDist * 22.0f);

// Мягкая светящаяся корона (шарик света) для крупных звезд  
float halo = 0.0f;  
if (hasHalo)  
{  
halo = exp(-normDist * 4.8f) * (0.28f + 0.45f * sizeClass);  
}

// Плавное затухание в 0 к краю радиуса (гарантия отсутствия резких ступенек)  
float edgeMask = 1.0f - smoothstep(0.65f, 1.0f, normDist);  
float profile = (core + halo) * edgeMask;

// Сложное двухволновое мерцание и переливание оттенка во времени  
float speed1 = 1.2f + hash.y * 3.8f;  
float speed2 = 2.7f + hash2.y * 4.5f;  
float phase1 = hash.z * 6.2831853f;  
float phase2 = hash2.z * 6.2831853f;

float wave = 0.62f + 0.23f * sin(time * speed1 + phase1) + 0.15f * sin(time * speed2 + phase2);  
// Легкое атмосферное переливание спектра при мерцании (сцинтилляция)  
float colorShift = frac(hash2.z + 0.06f * sin(time * (speed1 * 0.8f) + phase2));  
float3 specColor = ProceduralStars_SpectralColor(colorShift);

// Центр звезды горячее (белее), а ореол вокруг нее окрашен в индивидуальный цвет звезды  
float3 finalStarTint = lerp(specColor, float3(1.0f, 1.0f, 1.0f), saturate(core * 0.75f));

float starBrightness = baseHDR * (0.45f + 1.85f * sizeClass) * wave;  
float3 contribution = finalStarTint * (profile * starBrightness);

accumColor += contribution;  
accumAlpha = max(accumAlpha, saturate(profile * wave * 1.15f));  
}  
}

void ProceduralStars_float(  
float3 WorldPos,  
float3 LightForward,  
float StarsOpacity,  
float Time,  
out float4 ColorRGBA,  
out float AlphaOut)  
{  
float3 viewDir = normalize(WorldPos);

float3 starColor = float3(0.0f, 0.0f, 0.0f);  
float starAlpha = 0.0f;

if (viewDir.y > 0.005f && StarsOpacity > 0.001f)  
{  
// Слой 1: Мелкие далекие звезды-точки (только в 9% ячеек, высокая четкость сетки 260)  
ProceduralStars_EvaluateLayer(viewDir, 260.0f, 0.91f, 0.055f, 0.115f, 2.2f, false, Time, starColor, starAlpha);

// Слой 2: Средние выразительные звезды со слабым цветным ореолом (только в 4.5% ячеек, сетка 135)  
ProceduralStars_EvaluateLayer(viewDir, 135.0f, 0.955f, 0.075f, 0.165f, 4.0f, true, Time, starColor, starAlpha);

// Слой 3: Редкие яркие сияющие звезды-маяки (только в 1.5% ячеек, сетка 68, мягкий шарик свечения + HDR Bloom)  
ProceduralStars_EvaluateLayer(viewDir, 68.0f, 0.985f, 0.110f, 0.215f, 7.5f, true, Time, starColor, starAlpha);

// Плавное растворение звезд в воздушной дымке над горизонтом  
float horizonFade = smoothstep(0.03f, 0.20f, viewDir.y);  
starColor *= horizonFade;  
starAlpha *= horizonFade;  
}

starColor *= StarsOpacity;  
starAlpha *= StarsOpacity;

ColorRGBA = float4(starColor, starAlpha);  
AlphaOut = starAlpha;  
}

#endif // PROCEDURAL_STARS_INCLUDED