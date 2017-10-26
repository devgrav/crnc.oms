import * as React from "react";
import { Route, Switch } from "react-router";
import {BrowserRouter as Router} from "react-router-dom";
import EstimatesGrid from "./components/estimates/EstimatesGrid";
import JobsGrid from "./components/jobs/JobsGrid";
import NotFound from "./components/notFound/NotFound";
import UserCardEdit from "./components/users/UserCardEdit";
import UserCards from "./components/users/UserCards";

const Routes: React.StatelessComponent = () => {
    return (
        <Switch>
            <Route exact path="/" component={UserCards}/>
            <Route path="/users/:id" component={UserCards}/>
            <Route path="/users" component={UserCards}/>
            <Route path="/estimates" component={EstimatesGrid}/>
            <Route path="/jobs" component={JobsGrid}/>
            <Route component={NotFound}/>
        </Switch>
    );
};

export default Routes;
