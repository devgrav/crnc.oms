import * as React from "react";
import Pagination from "semantic-ui-react/dist/commonjs/addons/Pagination/Pagination";
import "./Paging.css";
import { Icon } from "semantic-ui-react";

export default class Paging
    extends React.Component<PagingProps>{

        constructor(props: any){
            super(props);
        }

        public render(){
            return (
                <Pagination
                    floated={this.props.floated}
                    activePage={this.props.activePage}
                    totalPages={this.props.totalPages}
                    className={this.props.vertical ? "verticalPaging" : undefined}
                    ellipsisItem={ this.props.vertical ?
                            { content: <Icon name="ellipsis vertical" />, icon: true } :
                            { content: <Icon name="ellipsis horizontal" />, icon: true }}
                    firstItem={this.props.vertical ?
                        { content: <Icon name="angle double up" />, icon: true } :
                        { content: <Icon name="angle double left" />, icon: true }}
                    lastItem={this.props.vertical ?
                        { content: <Icon name="angle double down" />, icon: true } :
                        { content: <Icon name="angle double right" />, icon: true}}
                    prevItem={this.props.vertical ?
                        { content: <Icon name="angle up" />, icon: true } :
                        { content: <Icon name="angle left" />, icon: true }}
                    nextItem={this.props.vertical ?
                        { content: <Icon name="angle down" />, icon: true } :
                        { content: <Icon name="angle right" />, icon: true }}
                />
            );
        }
}

interface PagingProps{
    totalPages: number;
    activePage?: number;
    vertical?: boolean;
    floated?: "right" | "left";
}
