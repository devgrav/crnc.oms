import { expect, type Page } from "@playwright/test";
import { fillField, selectOption } from "./form";
import { unique } from "./seed";

export interface CreatedOrder {
    jobDescription: string;
}

export async function createOrder(page: Page): Promise<CreatedOrder> {
    const jobDescription = unique("e2e order");

    await page.getByTestId("orders-add").click();
    await expect(page.getByTestId("order-card")).toBeVisible();

    await selectOption(page, "order-jobType", "New");
    await fillField(page, "order-jobDescription", jobDescription);
    await fillField(page, "order-customerTitle", "E2E Customer");
    await fillField(page, "order-customerAbbreviation", "EC");
    await fillField(page, "order-contactFirstName", "John");
    await fillField(page, "order-contactLastName", "Smith");
    await fillField(page, "order-contactEmail", "john_smith@crnc.com");
    await fillField(page, "order-contactPhone", "89161234567");

    await page.getByTestId("order-save").click();

    await expect(page.getByTestId("order-card")).toBeHidden();
    await expect(orderRow(page, jobDescription)).toBeVisible();

    return { jobDescription };
}

export function orderRow(page: Page, jobDescription: string) {
    return page.getByTestId("order-row").filter({ hasText: jobDescription });
}
