<p align="center">
  <img alt="logo" src="https://user-images.githubusercontent.com/37039414/192148237-65908b21-b509-47a1-ad68-b9a036c3a115.png">
</p>

[![Unity Version](https://img.shields.io/badge/unity-2021.3%2B-blue)](https://unity3d.com/get-unity/download)

## Содержание
- [О проекте](#о-проекте)
- [Схема подключения](#схема-подключения)
- [Калибровка](#калибровка)
- [Работа с приложением](#работа-с-приложением)
- [Зависимости](#зависимости)
- [Полезные ссылки](#полезные-ссылки)

## О проекте

Кроссплатформенное настольное приложение (cross-platform desktop application) для измерения электрического импеданса и RLC-характеристик (активное сопротивление, емкость и индуктивность) исследуемого радиоэлемента, 
подключаемого напрямую к звуковой карте компьютера.

## Схема подключения
Подключение исследуемого радиоэлемента осуществляется к двухканальному линейному вводу и одноканальному линейному выводу звуковой карты через опорный резистор **R<sub>ref</sub>**, 
сопротивление которого подбирается вручную и задается программно.

<p align="center">
  <img alt="soundcard-setup-scheme" src="https://user-images.githubusercontent.com/37039414/192144550-b61de31a-3faf-4cf5-a321-ffbf96c2fec8.png">
</p>

## Калибровка
Для уменьшения погрешности измерения импеданса предусмотрены следующие виды калибровочных испытаний:
- **GAIN**
  > Определяется разница коэффициента усиления между левым **V<sub>L</sub>** и правым **V<sub>R</sub>** каналами линейного входа для последующей компенсации 
  в режиме короткого замыкания в цепи с опорным резистором **R<sub>ref</sub>**.
- **OPEN**
  > Определяется входной импеданс звуковой карты **Z<sub>R</sub>** в режиме разомкнутой цепи с исследуемым импедансом **Z<sub>C</sub>**.
- **GROUND**
  > Определяется импеданс земли **Z<sub>G</sub>** в режиме короткого замыкания цепи с исследуемым импедансом **Z<sub>C</sub>**.

Схема замещения, соответствующая калибровке, и формулы рассчета исследуемого импеданса следующие:

<p align="center">
  <img alt="calibration-process-scheme" src="https://user-images.githubusercontent.com/37039414/192147615-559dac7a-302b-4f92-8ab7-23bbf5c16bf3.png">
</p>

## Работа с приложением
### 1. Конфигурация устройств ввода-вывода
Перед началом измерений требуется выбрать устройства ввода-вывода звуковой карты, используемые в ходе измерений.</br>
**Важно**: для проведения измерения необходимо, чтобы устройство ввода было двухканальным.</br>

После выбора устройств предлагается провести тестовую генерацию сигнала для определения среднеквадратичных значений сигнала на левом и правом каналах линейного ввода
и фазового сдвига между сигналами.

![soundcard-setup-screen](https://user-images.githubusercontent.com/37039414/192149437-7c312745-1574-47e8-89ff-9c213a3deef9.gif)

### 2. Конфигурация процесса измерения
Далее требуется настроить ключевые параметры процесса измерения
- сопротивление опорного резистора (**R<sub>ref</sub>**),
- опорный канал линейного ввода, который будет программно распознаваться как канал входного сигнала системы (для удобства, если при монтаже были спутаны каналы),
- частота дискретизации сигнала линейного вывода,

и установить частотный диапазон измерения. Для разовых измерений на определенной частоте предусмотрена соответствующая опция.

![measurement-setup-screen](https://user-images.githubusercontent.com/37039414/192149561-99e7d5eb-e01a-4589-bb9a-cd347bf7c259.gif)

### 3. Калибровочные испытания
Для большего качества измерений рекомендуется проведение необязательных калибровочных испытаний, суть которых описана выше.

![calibration-process-screen](https://user-images.githubusercontent.com/37039414/192150360-b9213ad2-7a66-475b-b2b8-8e72af812453.gif)

### 4. Проведение измерений
При отсутствии ошибок конфигурации на предыдущих этапах должно быть доступно проведение измерений. Процесс измерения в свою очередь проходит в несколько этапов:
1. Выбор следующей частоты синусоидального сигнала, начиная с нижней границы диапазона частот;
2. Генерация синусоидального сигнала выбранной частоты в течении некоторого времени переходного процесса, заданного в программе заранее;
3. Прослушивание левого и правого каналов линейного ввода и формирование соответствующих входного и выходного сигналов системы;
4. Рассчет исследуемого импеданса для входного и выходного сигналов системы с учетом результатов калибровки;
5. Повторение процедуры рассчета импеданса некоторое количество раз и усреднение полученного значения импеданса для заданной частоты.

![measurement-process-screen](https://user-images.githubusercontent.com/37039414/192150826-972cd43c-bc1d-43fc-aefd-3738beb77ac8.gif)

## Зависимости
- Для кросс-платформенной обработки ввода-вывода с поддержкой мультиканального ввода с низкой задержкой использовалась библиотека 
[jp.keijiro.libsoundio](https://github.com/keijiro/jp.keijiro.libsoundio);
- Для сохранения графиков импеданса в формате PNG при помощи нативного файлового проводника использовался плагин [UnityStandaloneFileBrowser](https://github.com/gkngkc/UnityStandaloneFileBrowser);
- Для логгирования ошибок, вспомогательных сведений времени выполнения используется плагин [UnityIngameDebugConsole](https://github.com/yasirkula/UnityIngameDebugConsole).

## Полезные ссылки
Для более пристального знакомства с проектом рекомендую к изучению следующие ресурсы, во многом послужившие источником вдохновения при разработке:
- https://www.daqarta.com/dw_0o0z.htm
- https://artalabs.hr/
