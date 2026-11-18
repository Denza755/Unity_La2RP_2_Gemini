# Архитектура и технический контекст проекта La2RP (Lineage 2 C1 на Unity 6 + Java L2J)

## 1. Структура папок на диске
• Основной клиент Unity (Unity 6 URP + UI Toolkit): C:\Unity_La2RP\l2-unity-main\l2-unity
• Java-сервер (протокол L2, порт 7777): C:\Unity_La2RP\La2RP_server
• Эталонный референс-клиент (для сверки механик): C:\Unity_La2RP\Gawric
• Распакованные исходники ассетов (.utx / текстуры): C:\Unity_La2RP\Resources
• Утилиты распаковки: C:\Unity_La2RP\l2encdec.exe, C:\Unity_La2RP\Umodelnew\umodel.exe

## 2. Графика, модели, снаряды и эффекты (Клиент)
• Приоритет загрузки оружия: Строгий приоритет папки Data/Animations/LineageWeapons перед assets/models/weapons (ModelTable.cs).
• Масштабы и сокеты рас:
	• Стандартные скелеты (MFighter и др.) и оружие LineageWeapons используют масштаб ~0.019 (1 / 52.5f).
	• Орки (MOrc, FOrc, MShaman, FShaman) из assets/models/characters/: сокеты Weapon_R_Bone, Weapon_L_Bone, Shield_L_Bone требуют локального смещения относительно Bip01 R/L Hand и компенсации масштаба GetOrcScaleFactor (~0.763f - 0.800f).
• Синхронизация «Меш + Материал» брони: Строго одинаковые индексы (m001 требует m001_*). При отсутствии меш и материал синхронно откатываются к голому телу (m000 + m000_t1000_*). Слоты кожи/ткани орков заполняются массивом sharedMaterials.
• Спекуляр и альфа (.tga / .utx): Суффикс _sp содержит карту блеска в альфа-канале. Для непрозрачной брони альфа принудительно переводится в 255 или отключается Alpha Clipping (_Smoothness: 0, Cull: 0).
• Стрелы и снаряды (Assets/Scripts/Game/Combat/Gear/Gear.cs, Assets/Scripts/Game/Manager/Effect/ProjectileManager.cs):
	• Стрелы (wooden_arrow_m00_et) вытянуты вдоль оси X; выравниваются в Gear.cs (CalculateArrowAlignmentData) острием вперед между правой и левой рукой.
	• Магические снаряды летят вдоль оси +Z (Quaternion.LookRotation) через ProjectileManager.cs.
• Динамический свет заклинаний (Assets/Scripts/Game/Manager/Effect/SpellDynamicLight.cs):
	• URP Point Light без теней для фаз Cast, Flight и Hit (настроены Wind Strike ID 1177, Self Heal ID 1216).
• Окружение: Сектора Говорящего Острова (17_25, 17_24, 16_24, 16_25) грузятся аддитивно. Небо: L2Sky.shadergraph + ProceduralStars.hlsl.

## 3. UI, тултипы, ввод и сетевой протокол
• UI Toolkit: Все окна на .uxml и .uss (L2Window, L2PopupWindow, L2GameUI.cs). Стили подключаются через Resources.Load<StyleSheet>.
• Система ввода: Строго новый UnityEngine.InputSystem (Keyboard.current, Mouse.current). Старый Input запрещен (возвращает ноль).
• Система тултипов (Assets/Scripts/UI/L2Tooltip.cs, Manipulators/Slot/TooltipManipulator.cs):
	• Менеджер: L2ToolTip : L2PopupWindow (L2Tooltip.cs). Манипулятор слотов передает ItemTooltipData (ItemId, Title).
	• Описания: Для скиллов (ComposeDescription) и баффов (effect.Description) уже активны. В ItemNameTable.cs (Assets/Scripts/Datatable/Grp/ItemNameTable.cs, читает StreamingAssets/Data/Meta/ItemName_Classic-eu.txt) распарсены description/trade/drop/sell, но в ItemTooltipData для предметов пока выводятся только Title и ID (описание предметов в тултипах не выведено).
• Сетевой протокол (Клиент: Assets/Scripts/Networking/ClientLibrary/PacketHandler/Gameserver/, Сервер: com.shnok.javaserver.gameserver.network):
	• Координаты L2 <-> Unity конвертируются через делитель 52.5f.
	• Обработчики пакетов: GameClientPacketHandler.cs (отправка с клиента), GameServerPacketHandler.cs (прием на клиенте), GamePacketHandler.java (демаршализация на сервере).
	• Ближний бой (MoveToPawn): дистанция проверяется в плоскости XZ с учетом collisionRadius. Взаимодействие с NPC: INTERACTION_DISTANCE = 150 в L2J.
	• Карта мира (Alt+M): RequestShowMiniMap (клиент) -> проверка World Map (ID 1665) -> ShowMiniMap (сервер).

## 4. Подсистема градостроительства и RTS-режима (Assets/Scripts/Ruler/)
• Клиент (Assets/Scripts/Ruler/):
	• RulerHUDWindow.cs: переключение режима, палитра из 30 префабов (Assets/Resources/Data/Buildings/, масштаб 0.65f), пагинация.
	• RulerCameraController.cs: RTS-камера на базе нового InputSystem.
	• BuildingPlacementSystem.cs + RulerGridOverlay.cs: проверка уклона (_maxSlopeHeightDelta = 1.6f), сетка, шейдеры призрака постройки, размещение PlacedBuildingInstance.cs.
	• RulerManager.cs + RulerSystemBootstrapper.cs: синглтон-контроллер, автозапуск, флаг allowAllPlayers = true (открыт для всех игроков).
• Сетевая синхронизация и база данных (Клиент + Сервер) — РЕАЛИЗОВАНО:
	• Клиентские пакеты: RequestRulerBuildActionPacket (опкод 0xee) — постройка/снос; RequestRulerCameraSyncPacket (опкод 0xef) — синхронизация положения камеры.
	• Серверный пакет: RulerBuildingsPacket (опкод 0xed) — трансляция состояния зданий клиентам (по чанкам и одиночно).
	• Сервер: RulerBuildingManager.java (таблица MySQL ruler_buildings, ALLOW_ALL_PLAYERS_BUILD = true, автосоздание таблицы, INSERT/DELETE).
• Этап 3 (Следующий шаг):
	• Валидация правки коллизий/запретных зон на стороне сервера, списание ресурсов/адены при строительстве, расширение прав модерации построек.

## 5. Правила генерации промптов для локального агента Antigravity
1. Всегда указывать точные абсолютные пути к папкам и целевым скриптам.
2. Запрещать слепой рекурсивный поиск файлов по всему диску через PowerShell (Get-ChildItem -Recurse), чтобы не перегружать сессию.
3. Формулировать задачи компактными атомарными блоками (по 1–3 связанных файла) с выполнением итеративно без остановок.
4. Обязательно требовать компиляцию в конце через dotnet build (клиент) и сборку .jar (при изменении Java-сервера).

## 6. Обязательный протокол памяти проекта (Auto-Update)
После успешного завершения любой крупной задачи (после прохождения dotnet build или сборки .jar сервера) агент ОБЯЗАН:
1. Открыть файл C:\Unity_La2RP\PROJECT_CONTEXT.md.
2. Дописать в конец раздела «7. Журнал последних изменений (Changelog)» 2–3 краткие строчки: дата, суть внедренной механики, затронутые файлы/пакеты.
3. Если механика изменила статус задачи (например, расширение тултипов или Ruler Этап 3), синхронно обновить соответствующий статус в разделах выше.

## 7. Журнал последних изменений (Changelog)
• [2026-11-18] Инициализация: сформирован базовый технический контекст проекта (Unity 6 URP, Java L2J, расы, магия, небо).
• [2026-11-18] Ревизия кода: уточнены пути SpellDynamicLight.cs (Game/Manager/Effect), Gear.cs, ProjectileManager.cs, ItemNameTable.cs. Уточнен статус Ruler: Этап 2 (БД ruler_buildings, пакеты 0xee/0xef/0xed, RulerBuildingManager.java) уже реализован. Описана архитектура тултипов (L2Tooltip.cs / TooltipManipulator.cs). Настроен протокол памяти проекта (.agent/rules/project_rules.md, .cursorrules).
• [2026-11-18] Исправление BuffWindow/DebuffWindow и синхронизация скиллов: устранены пробелы между слотами и сброс таймера на 60 сек (удаление истекших бафов при remainingDuration <= 0, фикс отрицательного времени в L2Tooltip.cs, полный .Clear() перед перерисовкой, flex-wrap: nowrap и margin: 1px в BuffWindow.uss; строгое разделение бафов и дебафов по Debuff/IconType). Из Gawric синхронизировано 25 недостающих скиллов (Skillgrp_Classic, SkillName_Classic-eu, шейдеры Data/Shaders/Skills, S_1177.cs, интерлюд-метафайлы). Успешная сборка dotnet build (0 ошибок).
• [2026-11-18] Сквозной перенос эффектов скиллов и глубокий аудит пайплайна: перенесено 57 директорий эффектов и анимаций LineageEffect из Gawric в Data/Effects. Доработан ParticleEffectTable.cs (снятие префикса ClassLineageEffect, таблица алиасов для вложенных префабов). Сгенерирован working_skills_audit.md (8272 скилла в базе, 7436 с валидными иконками, 30 с полным 4-уровневым пайплайном: метаданные + JSON + префабы + иконка + анимация). Сборка l2-unity.sln завершена с 0 ошибок.
• [2026-11-18] Полный сквозной перенос базы скиллов C1, мешей, текстур и классов эффектов: синхронизированы 510 записей Skillgrp и 508 SkillName из C1_Client (dec-skillgrp.txt, dec-skillname-e.txt). Распаковано 2054 иконки из Icon.utx в Data/UI/Icons и Data/SysTextures/Icon. Распаковано 117 текстур партиклов (WarEffectsTextures, LineageEffectsTextures) и 264 меша (LineageEffectsStaticmeshes, FX_E_S, FX_M_S, LineageEffectMeshes). Из Effects_C1 сгенерировано 171 префаб эффектов в Data/Particles/ и 35 JSON-конфигураций скиллов C1 в Data/LineageEffects и Data/Skill. В ParticleEffectTable.cs и SkillEffectTable.cs добавлена автозагрузка Data/Particles/ и Data/Skill/. В NewHumanoidAnimationController.cs и SkillgrpTable.cs расширен маппинг C1 анимаций (K, L, I, W, Y, MIX, защита от зависания). Сборка l2-unity.sln завершена с 0 ошибок.
• [2026-11-18] Исправление FormatException при парсинге метаданных: в Skillgrp_Classic.txt исправлены записи с дробным mp_consume (скилл 1194, 11.7 -> 12). В SkillgrpTable.cs и SkillNameTable.cs внедрен безопасный парсинг чисел ParseIntSafe с поддержкой округления float/InvariantCulture и Enum.TryParse с безопасными дефолтами (SkillType.Physical, SkillMagicType.None). Сборка l2-unity.sln завершена с 0 ошибок.
• [2026-11-18] Устранение NullReferenceException в SkillTable.CacheSkills: добавлен null-safe fallback для skillname и skillgrp (генерация дефолтного имени Skill_{ID} при отсутствии записи в SkillNameTable). В Skill.cs свойство EffectId защищено от пустого массива Skillgrps. В SkillName_Classic-eu.txt добавлены отсутствовавшие скиллы 3695 и 3696 (Squire's Shirt). Сборка l2-unity.sln завершена с 0 ошибок.
• [2026-11-18] Устранение NullReferenceException в ParticleManager.SpawnEffect: в ParticleManager.cs добавлено автодобавление компонента L2Particle при создании эффектов и хитов (AddComponent<L2Particle>), а также безопасная инициализация PooledEffect. В L2Particle.cs обеспечено автосоздание _pooledEffect в геттере и Awake, защита от null в ResetTimer(). В SpawnCastParticles заменен прерывающий return null на continue при невозможности спавна отдельного эффекта. Сборка l2-unity.sln завершена с 0 ошибок.
