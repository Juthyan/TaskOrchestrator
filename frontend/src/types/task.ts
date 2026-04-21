export type TaskStatus = 'Pending' | 'Running' | 'Succeeded' | 'Failed' | 'Cancelled' | 'Archived';
export type TaskType = 'Simulation' | 'Monitoring';

export interface Task {
    id: string;
    status: TaskStatus;
    type: TaskType;
    attempts: number;
    maxAttempts: number;
    createdAtUtc: string;
    lastUpdatedAtUtc: string;
}