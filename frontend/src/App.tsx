import { useQuery } from '@tanstack/react-query'
import { getAllTasks } from './api/tasks'
import { TaskForm } from './components/TaskForm'
import { TaskTable } from './components/TaskTable'
import { TaskMetrics } from './components/TaskMetrics'


function App() {
  const { data: tasks, isLoading } = useQuery({
    queryKey: ['tasks'],
    queryFn: getAllTasks,
    refetchInterval: 2000
  })

  return (
    <div className="min-h-screen bg-base-200 p-8">
      <div className="max-w-6xl mx-auto">
        <h1 className="text-4xl font-bold mb-8">TaskOrchestrator</h1>
        <TaskForm />
        <TaskMetrics tasks={tasks ?? []} />
        <div className="mt-8">
          {isLoading ? (
            <span className="loading loading-spinner loading-lg"></span>
          ) : (
            <TaskTable tasks={tasks ?? []} />
          )}
        </div>
      </div>
    </div>
  )
}

export default App