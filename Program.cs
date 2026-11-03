using App;
using TimeMachine.Core;

void SelectedItem(string text)
{
    Console.ForegroundColor = ConsoleColor.Black;
    Console.BackgroundColor = ConsoleColor.White;
    Console.WriteLine("> " + text);
}

void UnselectedItem(string text)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.BackgroundColor = ConsoleColor.Black;
    Console.WriteLine(text);
}

try
{
    (string, Action)[] items = [
        ("Записать данные", Screens.WriteData),
        ("Загрузить данные", Screens.LoadData),
        ("Сохранить данные", Screens.SaveData),
        ("Запустить сервис", () => {Screens.StartServiceAsync().GetAwaiter().GetResult(); }),
        ("Выход", () => Environment.Exit(0))
        ];
    var ci = 0;
    
    start:
    var current = SharedTimeProvider.Read();
    Console.Clear();
    Console.WriteLine("---Time machine---");
    Console.WriteLine("Меню:");
    if (current != DateTime.MinValue)
    {
        Console.WriteLine("Текущая дата: " + current.ToString("yyyy-MM-dd"));
        Console.WriteLine("Текущее время: " + current.ToString("HH:mm:ss"));
    }
    else
    {
        Console.WriteLine("Данные о дате и времени не загружены");
    }
    

    for(int i = 0; i < items.Length; i++)
    {
        if (ci == i) SelectedItem(items[i].Item1); else UnselectedItem(items[i].Item1);
    }
    Console.ResetColor();
    var key = Console.ReadKey(true).Key;
    if(key == ConsoleKey.DownArrow)
    {
        ci++;
        if(ci >= items.Length) ci = items.Length-1;
        goto start;
    }
    else if(key == ConsoleKey.UpArrow)
    {
        ci--;
        if (ci < 0) ci = 0;
        goto start;
    }
    else if(key == ConsoleKey.Enter)
    {
        items[ci].Item2();
        goto start;
    }
    else
    {
        goto start;
    }
    
}
catch(Exception ex)
{
    Console.WriteLine(ex.ToString());
}