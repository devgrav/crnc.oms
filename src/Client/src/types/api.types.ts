export interface TextValue {
    value: number;
    text: string;
}

export interface ItemsResponse<T> {
    // Пустая сетка приезжает без items вовсе.
    items?: T[];
}
