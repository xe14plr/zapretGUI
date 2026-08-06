<div align="center">

# 🗲 ZapretGUI

**Графическая оболочка для Flowseal/zapret-discord-youtube **

[![GitHub release](https://img.shields.io/github/v/release/xe14plr/zapretGUI?style=for-the-badge&color=7289da&logo=github)](https://github.com/xe14plr/zapretGUI/releases)
[![Target Platform](https://img.shields.io/badge/Platform-Windows_10_%7C_11-blue?style=for-the-badge&logo=windows)](https://github.com/xe14plr/zapretGUI)
[![Framework](https://img.shields.io/badge/Framework-.NET_8.0-512bd4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/xe14plr/zapretGUI?style=for-the-badge&color=green)](LICENSE)

---


---

![ZapretGUI — Обзор

</div>

## 📌 О проекте

**ZapretGUI** — это полнофункциональный GUI-клиент, превращающий классический консольный набор `.bat`-скриптов [Flowseal/zapret-discord-youtube](https://github.com/Flowseal/zapret-discord-youtube) в удобное приложение с поддержкой **Fluent Design & Mica**.

Вся подкапотная работа движка [zapret](https://github.com/bol-van/zapret) и драйвера [WinDivert](https://github.com/basil00/Divert) сохранена на 100%. Приложение не переписывает логику с нуля, а напрямую парсит оригинальные файлы `general*.bat`. Это позволяет обновлять пресеты просто перетаскиванием файлов из оригинального репозитория в папку `Strategies`.

---
> [!IMPORTANT]
> Все бинарные файлы в папке [`bin`](./bin) взяты из [zapret-win-bundle/zapret-winws](https://github.com/bol-van/zapret-win-bundle/tree/master/zapret-winws) и [zapret/releases](https://github.com/bol-van/zapret/releases). Вы можете это проверить с помощью хэшей/контрольных сумм. Проверяйте, что запускаете, используя сборки из интернета!
---

## ☑️Распространенные вопросы и проблемы

### После запуска скрипта `general*` ничего не происходит

- После запуска стратегии (отдельным bat файлом, не через service), должен открыться winws.exe (обход), который можно увидеть в панели задач.  
Если этого не произошло, то см. [#522](https://github.com/Flowseal/zapret-discord-youtube/issues/522)

### Не работает телеграм (веб версия) или бесконечное "подключение" к голосовому чату Discord
Запустите **`service.bat`**, выберите пункт **`Update hosts file`**. После чего, если ваш hosts будет неактуальным, то Вам будет предложено обновить его самостоятельно:  
  - Скопируйте весь текст из открывшегося блокнота
  - Откройте файл `hosts` в появившейся папке с помощью текстового редактора, открытого от имени администратора
  - Добавьте в конец файла `hosts` то, что скопировали (или замените, если до этого Вы уже добавляли подобное)
  - Сохраните и перепроверьте подключение. Если не работает - убедитесь, что файл `hosts` действительно сохранился.

### Обход не работает / перестал работать

> [!IMPORTANT]
> **Стратегии со временем могут переставать работать.**
> Определенная стратегия может работать какое-то время, но со временем она может переставать работать из-за обнаружения.
> В репозитории представлены множество различных стратегий для обхода. Если ни одна из них вам не помогает, то вам необходимо создать новую, взяв за основу одну из представленных здесь и изменив её параметры.
> Информацию про параметры стратегий вы можете найти [тут](https://github.com/bol-van/zapret/blob/master/docs/readme.md#nfqws).

- Проверьте, чтобы не было ошибок в `service.bat` -> `Run Diagnostics`

- Убедитесь, что адрес ресурса записан в списках доменов или IP

- Проверьте другие стратегии (**`ALT`**/**`FAKE`** и другие)
---

## 🚀 Быстрый старт

1. Перейдите в раздел **[Releases](../../releases/latest)** и скачайте свежий архив.
2. Распакуйте его в удобную папку *(избегайте кириллицы и пробелов в пути)*.
3. Запустите `ZapretGUI.exe` от имени **Администратора**.

> [!WARNING]
> **Предупреждение о защитных системах:**
> Драйвер **WinDivert** перехватывает сетевые пакеты для работы `winws.exe`, из-за чего антивирусы могут выдавать предупреждения (`Not-a-virus:RiskTool.Multi.WinDivert`). Это нормальное поведение легитимного инструмента. При возникновении проблем добавьте папку программы в исключения.
> 
> 📄 Подробнее см. в [README оригинального проекта](https://github.com/bol-van/zapret-win-bundle/blob/master/readme.md#%D0%B0%D0%BD%D1%82%D0%B8%D0%B2%D0%B8%D1%80%D1%83%D1%81%D1%8B).

---

## 🛠️ Сборка из исходников

Для самостоятельной сборки потребуется **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** и **Windows 10/11 x64**.

```bash
# 1. Клонирование репозитория
git clone [https://github.com/xe14plr/zapretGUI.git](https://github.com/xe14plr/zapretGUI.git)
cd zapretGUI/src

# 2. Обычная сборка
dotnet build

# 3. Публикация автономного (Self-Contained) .exe файла
dotnet publish ZapretGUI.App -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ../publish
