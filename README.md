# gang

State sharing algorithm.

A gang has a set of members running the same code with one member designated as the host.

All members can issue commands, these are sent to the current host. The host executes the command and broadcasts the current state to all members.

Should the host be disconnected from the gang, an other becomes the host. 

Gangs are controlled by a single relay server, but the work is done by the current host.