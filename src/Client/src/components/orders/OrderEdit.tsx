import * as React from "react";

export default class OrderEdit extends React.Component<any>{

    constructor(props: any){
        super(props);
    }

    public render(){

        return(
            <div>
                {`Edit estimate ${this.props.match.params.id}`}
            </div>
        );
    }
}
