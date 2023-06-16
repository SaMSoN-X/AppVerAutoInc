using System;
using System.IO;
using System.Text.RegularExpressions;

namespace AppVerAutoInc
{
    ////////////////////////////////////////////////////////////////////////////////
    //
    //          ╔═══╗        ╔╗  ╔╗       ╔═══╗     ╔╗     ╔══╗        
    //          ║╔═╗║        ║╚╗╔╝║       ║╔═╗║    ╔╝╚╗    ╚╣╠╝         
    //          ║║ ║║╔══╗╔══╗╚╗║║╔╝╔══╗╔═╗║║ ║║╔╗╔╗╚╗╔╝╔══╗ ║║ ╔═╗ ╔══╗
    //          ║╚═╝║║╔╗║║╔╗║ ║╚╝║ ║╔╗║║╔╝║╚═╝║║║║║ ║║ ║╔╗║ ║║ ║╔╗╗║╔═╝
    //          ║╔═╗║║╚╝║║╚╝║ ╚╗╔╝ ║║═╣║║ ║╔═╗║║╚╝║ ║╚╗║╚╝║╔╣╠╗║║║║║╚═╗
    //          ╚╝ ╚╝║╔═╝║╔═╝  ╚╝  ╚══╝╚╝ ╚╝ ╚╝╚══╝ ╚═╝╚══╝╚══╝╚╝╚╝╚══╝
    //               ║║  ║║ Application Version Autoincrementer                                           
    //               ╚╝  ╚╝                   
    //
    //                  +--------------------------------------+
    //                > Автоинкремент версии проекта C# WinForms <
    //                  +--------------------------------------+
    //
    //     Утилита перед каждой сборкой проекта анализирует файл AssembliInfo.cs
    // и в автоматическом режиме инкрементит версию проекта.
    //
    //     Маска версии проекта: x.y.z (например, 1.0.99), где 
    //                      
    //                    x - мажорная версия,
    //                    y - минорная версия, 
    //                    z - номер сборки. 
    //    
    //     При достижении номером сборки значения 1000 (z=1000), происходит 
    // инкремент минорной версии, а номер сборки обнуляется (т.е. после 
    // очередной сборки версия 1.0.999 получит номер 1.1.0); 
    // а также при достижении минорной версией значения 10 (y=10), 
    // происходит инкремент мажорной версии, а номер сборки обнуляется
    // (т.е. после очередной сборки версия 1.9.42 получит номер 2.0.0).
    //
    //     Примечание: значения, при которых происходит инкремент старших 
    // частей версии, легко меняются с помощью констант MAX_BUILD и MAX_MINOR.
    //
    //     Аргументы командной строки:
    //                    
    //                    args[0] - режим сборки (Debug или Release);
    //                    args[1] - название целевого проекта.
    //
    //                               +---------+
    //                             > How to use: <
    //                               +---------+
    //        
    //     1. Поместить в корневой каталог проекта C# WinForms рядом с 
    // файлом решения .sln. 
    //     2. Проект запускать с параметрами Projects Options -> Pre-Build Event 
    // (Свойства проекта -> События сборки): 
    //
    //  "$(SolutionDir)AppVerAutoInc.exe" $(ConfigurationName) "$(ProjectName)"
    //                                              ^                 ^
    //                                           args[0]           args[1]
    //
    //     3. Success! Версия проекта будет инкрементиться автоматически
    // при каждой сборке.
    //
    ////////////////////////////////////////////////////////////////////////////   
    class Program
    {
        /// <summary>
        /// Выводит в консоль справочную информацию.
        /// </summary>
        static void PrintManual()
        {
            Console.WriteLine("> How to use:");
            Console.WriteLine("1. Поместить в корневой каталог проекта C# WinForms рядом с файлом решения .sln.");
            Console.WriteLine("2. Проект запускать с параметрами Projects Options -> Pre-Build Event (Свойства проекта -> События сборки):");
            Console.WriteLine("\n    \"$(SolutionDir)AppVerAutoInc.exe\" $(ConfigurationName) \"$(ProjectName)\"\n");
            Console.WriteLine("3. Success! Версия проекта будет инкрементиться автоматически при каждой сборке.");
        }

        static void Main(string[] args)
        {
            try
            {
                // Параметры не переданы.
                if (args.Length < 2)
                {
                    Console.WriteLine("Параметры не переданы.");
                    PrintManual();
                    Console.ReadLine();
                    return;
                }

                // Режим сборки: Debug или Release.
                string buildMode = args[0];

                // Инкрементить версию будем только при сборке в режиме Release.
                if (buildMode == "Debug")
                    return;

                // Имя целевого проекта.
                string projectName = args[1];

                // Путь к файлу AssemblyInfo.cs проекта, в котором хранятся данные о версии проекта.
                string pathToAssemblyInfo = @"..\..\..\" + projectName + @"\Properties\AssemblyInfo.cs";

                // Поля версии.
                int major; // мажорная
                int minor; // минорная
                int build; // номер сборки

                // Константы максимальных значений полей версий, 
                // при которых происходит инкремент старших полей.
                const int MAX_BUILD = 999;
                const int MAX_MINOR = 9;


                string text = File.ReadAllText(pathToAssemblyInfo);

                Match match = new Regex("AssemblyVersion\\(\"(.*?)\"\\)").Match(text);
                Version ver = new Version(match.Groups[1].Value);

                if (ver.Build == MAX_BUILD)
                {
                    build = 0;
                    minor = ver.Minor + 1;
                }
                else
                {
                    build = ver.Build + 1;
                    minor = ver.Minor;
                }

                if (minor == MAX_MINOR + 1)
                {
                    major = ver.Major + 1;
                    minor = 0;
                    build = 0;
                }
                else
                {
                    major = ver.Major;
                }

                Version newVer = new Version(major, minor, build);

                text = Regex.Replace(text, @"AssemblyVersion\((.*?)\)", "AssemblyVersion(\"" + newVer.ToString() + "\")");
                text = Regex.Replace(text, @"AssemblyFileVersionAttribute\((.*?)\)", "AssemblyFileVersionAttribute(\"" + newVer.ToString() + "\")");
                text = Regex.Replace(text, @"AssemblyFileVersion\((.*?)\)", "AssemblyFileVersion(\"" + newVer.ToString() + "\")");

                File.WriteAllText(pathToAssemblyInfo, text);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n");
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }
        }
    }
}
