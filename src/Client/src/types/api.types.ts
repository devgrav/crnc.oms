// Опция выпадающего списка в том виде, в каком её отдают бэкенды.
// Значение числовое: enum'ы в контрактах Sales намеренно не строковые.
export interface TextValue {
    value: number;
    text: string;
}

export interface ItemsResponse<T> {
    items: T[];
}
