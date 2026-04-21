import { PieChart, Pie, Cell, Legend, Tooltip } from 'recharts'
import type { Task } from '../types/task'

interface TaskMetricsProps {
    tasks: Task[]
}

const COLORS: Record<string, string> = {
    Succeeded: '#22c55e',
    Failed: '#ef4444',
    Pending: '#f59e0b',
    Running: '#3b82f6',
    Cancelled: '#6b7280',
    Archived: '#8b5cf6',
}

export function TaskMetrics({ tasks }: TaskMetricsProps) {
    const statusCounts = tasks.reduce((acc, task) => {
        acc[task.status] = (acc[task.status] ?? 0) + 1
        return acc
    }, {} as Record<string, number>)

    const data = Object.entries(statusCounts).map(([name, value]) => ({ name, value }))

    return (
        <div className="card bg-base-100 shadow p-6">
            <h2 className="text-xl font-bold mb-4">Task Status Distribution</h2>
            <PieChart width={400} height={300}>
                <Pie
                    data={data}
                    dataKey="value"
                    nameKey="name"
                    cx="50%"
                    cy="50%"
                    outerRadius={100}
                >
                    {data.map((entry) => (
                        <Cell key={entry.name} fill={COLORS[entry.name] ?? '#94a3b8'} />
                    ))}
                </Pie>
                <Tooltip />
                <Legend />
            </PieChart>
        </div>
    )
}