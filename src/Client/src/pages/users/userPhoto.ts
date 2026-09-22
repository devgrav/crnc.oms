import type { UserItem } from "@/types/users.types";

export function photoSrc(user: Pick<UserItem, "photoBase64" | "photoMimeType">): string | undefined {
    if (!user.photoBase64) {
        return undefined;
    }

    return `data:${user.photoMimeType ?? "image/jpeg"};base64,${user.photoBase64}`;
}

export interface SelectedPhoto {
    photoBase64: string;
    photoMimeType: string;
}

export async function readPhoto(file: File): Promise<SelectedPhoto> {
    const dataUrl = await readAsDataUrl(file);
    const separator = dataUrl.indexOf(",");

    return {
        photoBase64: dataUrl.slice(separator + 1),
        photoMimeType: file.type,
    };
}

function readAsDataUrl(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();

        reader.onload = () => {
            resolve(typeof reader.result === "string" ? reader.result : "");
        };
        reader.onerror = () => { reject(new Error("Could not read the selected file")); };
        reader.readAsDataURL(file);
    });
}
