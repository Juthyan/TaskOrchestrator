import type { Task, TaskType } from '../types/task';

const API_URL = 'https://taskorchestrator-production-02e6.up.railway.app';

export async function getTask(id: string): Promise<Task> {
    const response = await fetch(`${API_URL}/tasks/${id}`);
    return response.json();
}

export async function getAllTasks(): Promise<Task[]> {
    const response = await fetch(`${API_URL}/tasks`);
    return response.json();
}

export async function createTask(type: TaskType, maxAttempts: number): Promise<{ id: string }> {
    const response = await fetch(`${API_URL}/tasks`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ type, maxAttempts }),
    });
    return response.json();

}

export async function cancelTask(id: string): Promise<void> {
    const response = await fetch(`${API_URL}/tasks/${id}/cancel`, {
        method: 'POST',
    });

    if (!response.ok) {
        const data = await response.json();
        throw new Error(data.error);
    }
}

export async function restartTask(id: string): Promise<{ id: string }> {
    const response = await fetch(`${API_URL}/tasks/${id}/restart`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
    });

    if (!response.ok) {
        const data = await response.json();
        throw new Error(data.error);
    }

    return response.json();
}
