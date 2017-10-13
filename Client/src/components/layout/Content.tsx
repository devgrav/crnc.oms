import * as React from "react";
import { Route, Switch } from "react-router";
import {Header} from "semantic-ui-react";
import EstimatesGrid from "../estimates/EstimatesGrid";
import UsersGrid from "../users/UsersGrid";

const Content: React.StatelessComponent = (props: any) => {
    return (
        <main>
            {props.children}
        </main>
    );
};

export default Content;
