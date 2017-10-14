import * as React from "react";

export default class UserEdit extends React.Component<any>{

    constructor(props: any){
        super(props);
    }

    public render(){

        return(
            <div>
                {`Edit user ${this.props.match.params.id}`}
            </div>
        );
    }
}
