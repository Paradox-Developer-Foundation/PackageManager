namespace Hoi4PackageManager.Configurations;

public static class AppConfigurations
{
    public static readonly string LibPath = System.IO.Path.Join(
        System.Environment.CurrentDirectory,
        "lib"
    );
    public static readonly string DbPath = System.IO.Path.Join(LibPath, @"sqlite\packages.db");
    public static readonly string FilesPath = System.IO.Path.Join(
        System.Environment.CurrentDirectory,
        "files"
    );
}
