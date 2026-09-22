import { Alert, Container } from "@mantine/core";

export default function NotFound() {
    return (
        <Container mt="lg">
            <Alert color="blue" title="Page not found" data-testid="not-found" />
        </Container>
    );
}
