import { describe, expect, it } from "vitest";
import { appVersion, formatAppVersion } from "../appVersion";

describe("appVersion", () => {
    it("AppVersion_FromBuild_IsSubstituted", () => {
        //Arrange, Act, Assert - значение приходит из package.json через define,
        //а не из константы в коде: если подстановка отвалится, здесь будет
        //необработанный идентификатор или пустая строка
        expect(appVersion).toMatch(/^\d+\.\d+\.\d+/);
    });

    it("FormatAppVersion_LocalBuild_ShowsVersionOnly", () => {
        //Arrange, Act
        const label = formatAppVersion("1.2.3", "dev");

        //Assert
        expect(label).toBe("v1.2.3");
    });

    it("FormatAppVersion_CiBuild_AppendsCommit", () => {
        //Arrange - в CI коммит известен и попадает в подпись
        const label = formatAppVersion("1.2.3", "a1b2c3d");

        //Assert
        expect(label).toBe("v1.2.3 · a1b2c3d");
    });
});
