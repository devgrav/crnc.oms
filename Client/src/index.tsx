import * as React from "react";
import * as ReactDOM from "react-dom";
import { AppContainer } from "react-hot-loader";
import { BrowserRouter as Router } from "react-router-dom";
import App from "./app";

ReactDOM.render(
    <AppContainer>
        <App/>
    </AppContainer>,
    document.getElementById("root") as HTMLElement
);

interface RequireImport {
    default: any;
}

if (module.hot) {
    module.hot.accept("./app", () => {
        const NextApp = require<RequireImport>("./app").default;
        ReactDOM.render(
            <AppContainer>
                <NextApp />
            </AppContainer>,
            document.getElementById("root")
        );
    });
}
