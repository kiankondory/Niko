// مسئولیت: پیکربندی APK companion برای Wear OS.
// وابستگی‌ها و لایه: Android adapter؛ contract و queue محلی، بدون domain logic.
// نکات تغییر و قیود: هیچ SDK هوش مصنوعی، backend، billing یا credential استفاده نمی‌شود.
import org.jetbrains.kotlin.gradle.dsl.JvmTarget

plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
}

android {
    namespace = "com.companyname.niko.wear"
    compileSdk = 35

    defaultConfig {
        applicationId = "com.companyname.niko.wear"
        minSdk = 30
        targetSdk = 35
        versionCode = 1
        versionName = "1.0"
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_1_8
        targetCompatibility = JavaVersion.VERSION_1_8
    }

    kotlin {
        compilerOptions {
            jvmTarget.set(JvmTarget.JVM_1_8)
        }
    }

    buildFeatures {
        buildConfig = false
    }
}

dependencies {
    implementation("com.google.android.gms:play-services-wearable:18.2.0")
    testImplementation("junit:junit:4.13.2")
    testImplementation("org.json:json:20240303")
}
