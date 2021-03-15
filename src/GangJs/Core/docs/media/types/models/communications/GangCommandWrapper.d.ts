import { IGangCommand } from './IGangCommand';
/** wraps command data */
export declare class GangCommandWrapper<T> implements IGangCommand {
    /** command data type */
    readonly type: string;
    /** command data */
    readonly data: T;
    /** sequence number */
    readonly sn: number;
    /** in reply to sequence number */
    readonly rsn: number;
    constructor(
    /** command data type */
    type: string, 
    /** command data */
    data: T, 
    /** sequence number */
    sn?: number, 
    /** in reply to sequence number */
    rsn?: number);
}
