import * as React from "react";
import { Route, Switch } from "react-router";
import EstimatesGrid from "./components/estimates/EstimatesGrid";
import JobsGrid from "./components/jobs/JobsGrid";
import NotFound from "./components/notFound/NotFound";;
import UserCards from "./components/users/UserCards";
import EstimateEdit from "./components/estimates/EstimateEdit";
import Login from "./components/auth/Login";

const Routes: React.StatelessComponent = () => {
    return (
        <Switch>
            <Route exact path="/" component={UserCards}/>
            <Route path="/users/:id" component={UserCards}/>
            <Route path="/users" component={UserCards}/>
            <Route path="/estimates/:id" component={EstimateEdit}/>
            <Route path="/estimates" component={EstimatesGrid}/>
            <Route path="/jobs" component={JobsGrid}/>
            <Route path="/login" component={Login}/>
            <Route component={NotFound}/>
        </Switch>
    );
};

export default Routes;
