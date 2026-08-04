<div align="center">

# 🗲 ZapretGUI

**Современная графическая оболочка в стиле Windows 11 Fluent Design для обхода DPI-блокировок**

[![GitHub release](https://img.shields.io/github/v/release/xe14plr/zapretGUI?style=for-the-badge&color=7289da&logo=github)](https://github.com/xe14plr/zapretGUI/releases)
[![Target Platform](https://img.shields.io/badge/Platform-Windows_10_%7C_11-blue?style=for-the-badge&logo=windows)](https://github.com/xe14plr/zapretGUI)
[![Framework](https://img.shields.io/badge/Framework-.NET_8.0-512bd4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/xe14plr/zapretGUI?style=for-the-badge&color=green)](LICENSE)

---

[О проекте](#-о-проекте) • [Возможности](#-возможности) • [Быстрый старт](#-быстрый-старт) • [Сборка](#-сборка-из-исходников) • [Благодарности](#-благодарности)

---

![ZapretGUI — Обзор](docs/screenshot-dashboard.png)

</div>

## 📌 О проекте

**ZapretGUI** — это полнофункциональный GUI-клиент, превращающий классический консольный набор `.bat`-скриптов [Flowseal/zapret-discord-youtube](https://github.com/Flowseal/zapret-discord-youtube) в удобное приложение с поддержкой **Fluent Design & Mica**.

Вся подкапотная работа движка [zapret](https://github.com/bol-van/zapret) и драйвера [WinDivert](https://github.com/basil00/Divert) сохранена на 100%. Приложение не переписывает логику с нуля, а напрямую парсит оригинальные файлы `general*.bat`. Это позволяет обновлять пресеты просто перетаскиванием файлов из оригинального репозитория в папку `Strategies`.

---

## ✨ Возможности

<table>
  <tr>
    <td width="50%">
      <h3>🖥️ Панель управления</h3>
      <ul>
        <li>Мгновенный запуск и остановка обхода в один клик.</li>
        <li>Установка и удаление службы Windows (автозапуск).</li>
        <li>Быстрое переключение между активными стратегиями.</li>
      </ul>
    </td>
    <td width="50%">
      <h3>⚙️ Стратегии и настройки</h3>
      <ul>
        <li>Живой предпросмотр итоговой команды <code>winws.exe</code>.</li>
        <li>Управление <b>Game Filter</b> и режимами <b>IPSet Filter</b>.</li>
        <li>Встроенный редактор пользовательских списков <code>*-user.txt</code>.</li>
      </ul>
    </td>
  </tr>
  <tr>
    <td width="50%">
      <h3>🩺 Глубокая диагностика</h3>
      <ul>
        <li>Автопоиск конфликтов (VPN, Check Point, Killer, SmartByte).</li>
        <li>Проверка служб BFE, системного прокси, TCP timestamps, DoH.</li>
        <li>Быстрая очистка зависших служб WinDivert и кэша Discord.</li>
      </ul>
    </td>
    <td width="50%">
      <h3>🎨 Сопровождение и стиль</h3>
      <ul>
        <li>Адаптивный интерфейс: тёмная и светлая темы с эффектом Mica.</li>
        <li>Автоматическая проверка обновлений наборов стратегий.</li>
        <li>Полная независимость бизнес-логики от UI.</li>
      </ul>
    </td>
  </tr>
</table>

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
