import * as React from "react";
import { Route, Switch } from "react-router";
import {BrowserRouter as Router} from "react-router-dom";
import EstimatesGrid from "./components/estimates/EstimatesGrid";
import JobsGrid from "./components/jobs/JobsGrid";
import NotFound from "./components/notFound/NotFound";
import UserCards from "./components/users/UserCards";
import UserEdit from "./components/users/UserEdit";
import UsersGrid from "./components/users/UsersGrid";

const Routes: React.StatelessComponent = () => {
    return (
        <Switch>
            <Route exact path="/" component={UsersGrid}/>
            <Route path="/users/:id" component={UserEdit}/>
            <Route path="/userCards" component={UserCards}/>
            <Route path="/users" component={UsersGrid}/>
            <Route path="/estimates" component={EstimatesGrid}/>
            <Route path="/jobs" component={JobsGrid}/>
            <Route component={NotFound}/>
        </Switch>
    );
};

export default Routes;
