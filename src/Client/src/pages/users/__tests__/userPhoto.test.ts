import { describe, expect, it } from "vitest";
import { photoSrc, readPhoto } from "../userPhoto";

describe("photoSrc", () => {
    it("PhotoBase64_Present_BuildsDataUri", () => {
        //Arrange - Security отдаёт фото как base64 плюс mime-тип отдельным полем
        const user = { photoBase64: "/9j/4AAQ", photoMimeType: "image/jpeg" };

        //Act
        const src = photoSrc(user);

        //Assert - ни пробелов, ни переносов внутри: иначе браузер не отрисует
        expect(src).toBe("data:image/jpeg;base64,/9j/4AAQ");
    });

    it("PhotoBase64_Missing_ReturnsUndefined", () => {
        //Arrange, Act
        const src = photoSrc({ photoBase64: undefined, photoMimeType: undefined });

        //Assert - Avatar покажет заглушку
        expect(src).toBeUndefined();
    });

    it("MimeType_Missing_FallsBackToJpeg", () => {
        //Arrange, Act
        const src = photoSrc({ photoBase64: "AAA", photoMimeType: undefined });

        //Assert
        expect(src).toBe("data:image/jpeg;base64,AAA");
    });
});

describe("readPhoto", () => {
    it("SelectedFile_Always_StripsDataUriPrefix", async () => {
        //Arrange - на сервер уходит чистый base64, без префикса data:
        const file = new File([new Uint8Array([1, 2, 3])], "avatar.png", { type: "image/png" });

        //Act
        const photo = await readPhoto(file);

        //Assert
        expect(photo.photoMimeType).toBe("image/png");
        expect(photo.photoBase64).not.toContain("data:");
        expect(photo.photoBase64).toBe(btoa(""));
    });
});
