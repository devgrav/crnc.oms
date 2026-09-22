import { Alert, Container } from "@mantine/core";

export default function Forbidden() {
    return (
        <Container mt="lg">
            <Alert color="blue" title="You have not access to this page" data-testid="forbidden" />
        </Container>
    );
}
