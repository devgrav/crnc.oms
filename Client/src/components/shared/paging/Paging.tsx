import * as React from "react";
import Pagination from "semantic-ui-react/dist/commonjs/addons/Pagination/Pagination";
import "./Paging.css";
import { Icon, PaginationProps } from "semantic-ui-react";

export default class Paging
    extends React.Component<PagingProps>{

        constructor(props: any){
            super(props);

            this.onPageChange = this.onPageChange.bind(this);
        }

        getClassName(){
            let classNames = [];

            if(this.props.vertical)
                classNames.push("verticalPaging");

            if(this.props.primary){
                classNames.push("primary");
            }

            if(classNames.length === 0)
                return undefined;

            return classNames.join(" ");
        }

        onPageChange(event: React.MouseEvent<HTMLAnchorElement>, data: PaginationProps){
            if(data.activePage)
                this.props.onPageChange(data.activePage as number);
            else
                this.props.onPageChange(1);
        }

        public render(){
            return (
                <Pagination
                    floated={this.props.floated}                    
                    activePage={this.props.activePage}
                    totalPages={this.props.totalPages}
                    className={this.getClassName()}
                    onPageChange={this.onPageChange}
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
    primary?: boolean
    floated?: "right" | "left";
    onPageChange(activePage: number): void;
}
