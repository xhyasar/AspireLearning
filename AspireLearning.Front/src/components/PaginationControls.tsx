// src/components/PaginationControls.tsx
export default function PaginationControls({
                                               page,
                                               total,
                                               perPage,
                                               setPage,
                                           }: {
    page: number;
    total: number;
    perPage: number;
    setPage: (n: number) => void;
}) {
    const max = Math.ceil(total / perPage);
    return (
        <div className="flex justify-between mt-4 text-sm">
            <span>Page {page} of {max}</span>
            <div className="flex gap-2">
                <button onClick={() => setPage(page - 1)} disabled={page <= 1}>←</button>
                <button onClick={() => setPage(page + 1)} disabled={page >= max}>→</button>
            </div>
        </div>
    );
}
