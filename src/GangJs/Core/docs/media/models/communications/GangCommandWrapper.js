/** wraps command data */
export class GangCommandWrapper {
    constructor(
    /** command data type */
    type, 
    /** command data */
    data, 
    /** sequence number */
    sn = undefined, 
    /** in reply to sequence number */
    rsn = undefined) {
        this.type = type;
        this.data = data;
        this.sn = sn;
        this.rsn = rsn;
    }
}
