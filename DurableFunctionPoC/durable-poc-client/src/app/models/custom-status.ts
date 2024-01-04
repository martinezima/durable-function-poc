import { ICustomStatusDetails } from "./custom-status-details";

export interface ICustomStatus {
    Activity: string;
    Status: string,
    Details: ICustomStatusDetails [];
}