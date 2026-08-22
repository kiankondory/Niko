// مسئولیت: تنظیمات پروژهٔ مستقل Wear OS Niko.
// وابستگی‌ها و لایه: Gradle native adapter؛ به Core/SQLite یا backend متصل نیست.
// نکات تغییر و قیود: پروژهٔ Wear فقط قرارداد Companion را حمل می‌کند و منبع حقیقت Phone است.
pluginManagement {
    repositories {
        google()
        mavenCentral()
        gradlePluginPortal()
    }
}

dependencyResolutionManagement {
    repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS)
    repositories {
        google()
        mavenCentral()
    }
}

rootProject.name = "Niko.Wear"
include(":app")
