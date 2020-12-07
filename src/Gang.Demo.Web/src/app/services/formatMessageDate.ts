const messageDateFormatter = Intl.DateTimeFormat('en-GB', {
  weekday: 'short',
  year: 'numeric',
  month: 'short',
  day: 'numeric',
  hour: 'numeric',
  minute: 'numeric'
}).format;

export function formatMessageDate(date: string | number) {
  if (date == null || isNaN(date as any))
    return '';

  return messageDateFormatter(new Date(date));
}
