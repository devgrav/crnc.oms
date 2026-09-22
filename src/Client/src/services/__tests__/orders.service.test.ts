import { AxiosError, AxiosHeaders, type AxiosResponse } from "axios";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { createOrder, getOrders, updateOrder } from "../orders.service";
import apiClient from "../apiClient";
import { makeOrderRow } from "@/test/factories";

vi.mock("../apiClient", () => ({
    default: {
        get: vi.fn(),
        post: vi.fn(),
        put: vi.fn(),
        delete: vi.fn(),
    },
}));

const client = vi.mocked(apiClient);

describe("orders.service", () => {
    beforeEach(() => { vi.resetAllMocks(); });

    it("GetOrders_ItemsEnvelope_UnwrapsItems", () => {
        //Arrange - Sales отдаёт { items: [...] }, а не голый массив
        const order = makeOrderRow();
        client.get.mockResolvedValue({ data: { items: [order] } });

        //Act, Assert
        return getOrders().then((result) => {
            expect(result.success).toBe(true);
            expect(result.data).toEqual([order]);
        });
    });

    it("GetOrders_MissingItems_ReturnsEmptyArray", async () => {
        //Arrange - пустой ответ не должен ронять сетку
        client.get.mockResolvedValue({ data: {} });

        //Act
        const result = await getOrders();

        //Assert
        expect(result.data).toEqual([]);
    });

    it("CreateOrder_ValidationFailure_ReturnsResultInsteadOfThrowing", async () => {
        //Arrange
        client.post.mockRejectedValue(badRequest({ errors: { jobType: ["Required"] } }));

        //Act
        const result = await createOrder({
            jobType: 0,
            jobDescription: "",
            customerTitle: "",
            customerAbbreviation: "",
            customerContactPersonFirstName: "",
            customerContactPersonLastName: "",
            customerContactPersonEmail: "",
            customerContactPersonPhone: "",
        });

        //Assert - наружу не летит исключение, поэтому в компонентах нет try/catch
        expect(result.success).toBe(false);
        expect(result.fieldErrors).toEqual({ jobType: ["Required"] });
    });

    it("UpdateOrder_Always_SendsIdInPayload", async () => {
        //Arrange - PUT /api/orders адресует заказ телом, а не путём
        client.put.mockResolvedValue({ data: undefined });

        //Act
        await updateOrder({
            id: "order-1",
            jobType: 1,
            jobDescription: "Wall",
            customerTitle: "ACME",
            customerAbbreviation: "AC",
            customerContactPersonFirstName: "John",
            customerContactPersonLastName: "Smith",
            customerContactPersonEmail: "john@crnc.com",
            customerContactPersonPhone: "89161234567",
        });

        //Assert
        expect(client.put).toHaveBeenCalledWith(
            "/sales/orders",
            expect.objectContaining({ id: "order-1" }),
        );
    });

    it("GetOrders_NetworkFailure_ReportsConnectionProblem", async () => {
        //Arrange
        client.get.mockRejectedValue(new AxiosError("Network Error"));

        //Act
        const result = await getOrders();

        //Assert
        expect(result.success).toBe(false);
        expect(result.generalError).toContain("Cannot reach the server");
    });

    it("GetOrders_RelativePath_DoesNotCarryBackendHost", async () => {
        //Arrange - адрес бэкенда не хардкодится: разводит пути nginx или dev-прокси
        client.get.mockResolvedValue({ data: { items: [] } });

        //Act
        await getOrders();

        //Assert
        expect(client.get).toHaveBeenCalledWith("/sales/orders");
    });
});

function badRequest(data: unknown): AxiosError {
    const headers = new AxiosHeaders();
    const config = { headers };
    const response = { status: 400, data, statusText: "", headers, config } as AxiosResponse;

    return new AxiosError("Request failed", "400", config, null, response);
}
