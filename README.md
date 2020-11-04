# IMRY: I'll Make Reports for You

![](https://camo.githubusercontent.com/6e386aa932b31a8f5281f92f93d6c210569afc13/68747470733a2f2f6170692e6369727275732d63692e636f6d2f6769746875622f686967616e2d656d752f686967616e2e7376673f7461736b3d77696e646f77732d7838365f36342d62696e6172696573)
## Описание проекта
IMRY это программа для автоматического создания Word отчетов. Отчеты создаются из шаблонов и конфигурационных файлов.
Шаблон - набор лабораторных и практических работ, в которых указаны название, цель, теоретическая часть, общее задание и индивидуальные задания.
 
## Установка
### [`>>> СКАЧАТЬ <<<`](InstallationFiles/Installer/Installer-SetupFiles/Installer.exe?raw=true)
Для установки программы можно скачать из папки **```InstallationFiles/Installer/Installer-SetupFiles```** файл **```Installer.exe```**, или просто нажать на большую кнопку сверху. При открытии файла запустится стандартный установщик, нужно будет проследовать по всем шагам. После установки программма будет отображаться в списке установленных программ, появится иконка на рабочем столе и в меню «Пуск».

## Запуск программы
Чтобы запустить программу, можно воспользоваться ярлыком на рабочем столе или в меню «Пуск».

---
## Реализованный функционал
- [x] Создание отчетов
- [x] Создание, сохранение, изменение шаблонов для отчетов
- [x] Сохранение и загрузка информации о студенте
- [x] Переход к папке с отчетами после создания отчета
- [x] Кнопка для быстрого перехода к репозиторию для обновления
- [x] Окно с настройками для изменения главного конфигурационного файла
---
## В разработке
- [ ] Возможность сохранять и загружать введенные данные при генерации отчета
- [ ] Возможность выбирать расширения файлов для добавления при использовании Drag & Drop для каждой работы
---
## *Работу программы можно изменять, используя конфигурационные файлы.*
## Описание конфигурационных файлов
Для работы программы **нужен главный конфиругационный файл** в папке приложения, который содержит следующие параметры:
| Параметр                   | Тип данных | Описание параметра                 | Тип данных в файле, если параметр - путь |
| -------------------------- | -------- | ------------------------------------------------------ | -------------------------------- |
|WorkHasTitlePage            | bool     | Есть ли в работе титульная страница                    | Параметр - не путь               |
|WorkTitlePageFilePath       | string   | Путь до файла с титульником (при наличии)              | ```Dictionary<string, string>``` |
|WorkHasTitlePageParams      | bool     | Есть ли в работе в титульнике какие-то параметры       | Параметр - не путь               |
|WorkTitlePageParamsFilePath | string   | Путь до файл с параметрами для титульника (при наличии)| ```List<string>```              |
|AllReportsPath              | string   | Путь до папки, где все отчеты будут сохраняться  | Параметр - не путь         |
|PermittedDragAndDropExtentionsFilePath | string | Разрешенные расширения файлов при использовании Drag & Drop | ```List<string>```
|CurrentTemplateFilePath     | string   | Текущий шаблон для работ | ```Dictionary<string, Dictionary<string, ReportInformation>>``` ( лучше создавать в программе в пунке меню "Создать шаблон" ) |
|UserDataFilePath            | string   | Путь до файла с сохраненными данными пользователя | ```StudentInformation``` ( создается программой ) |


### Пример главного конфигурационного файла
```json
{
  "WorkHasTitlePage": true,
  "WorkTitlePageFilePath": "./Configs/TitlePage.docx",
  "WorkHasTitlePageParams": true,
  "WorkTitlePageParamsFilePath": "./Configs/TitlePageParams.json",
  "PermittedDragAndDropExtentionsFilePath": "./Configs/PermittedDragAndDropExtentions.json",
  "CurrentTemplateFilePath": "./Subjects/Java/JavaTasks.json",
  "UserDataFileName": "",
  "AllReportsPath": "./Reports/"
}
```

### Пример файла с разрешенными расширениями файлов для Drag & Drop
### **При наличии \*, будут добавляться все файлы**
```json
[
    "срр",
    "h",
    "cs",
    "java",
    "py",
    "asm",
    "png",
    "bmp",
    "jpg",
    "jpeg"
]
```
### *При создании отчета, можно использовать следующие константы на титульной странице*
## ***Все константы должны быть вставлены в формате {{НазваниеКонстанты}}***
---
### Таблица констант (Заменяются на текст)

| Константа        | Описание                      | На что будет заменена                         |
| ---------------- | ----------------------------- | --------------------------------------------- |
| WorkType         | Тип работы                    | "Практическая работа" / "Лабораторная работа" |
| WorkNumber       | Номер работы                  | Число                                         |
| WorkName         | Полное название работы        | Строка ( значение берется из шаблона )        |
| WorkTheoryPart   | Теоретическая часть работы    | Строка ( значение берется из шаблона )        |
| WorkTarget       | Цель работы                   | Строка ( значение берется из шаблона )        |
---
### Таблица специальных констант (Заменяются на несколько абзацев с картиками (при наличии))
| Константа    | Описание                                       |
| ------------ | ---------------------------------------------- |
| CommonTask   | Общее описание работы                          |
| DynamicTasks | Выбранные задания в работе (при наличии)       |
| UserFiles    | Файлы, добавленные пользователем (при наличии) |

### При заполнении шаблонов, можно также вставить изображения, изображение вставляется в виде ```{{image source="Относительный/абсолютный путь", name="Подпись картинки (можно не указывать)"}}```. Картинки нумеруются автоматически. 

## Удаление
Для удаления программы можно воспользоваться любым деинсталлятором. Также можно в меню «Пуск» нажать правой кнопкой по иконке приложения, затем "Удалить".
