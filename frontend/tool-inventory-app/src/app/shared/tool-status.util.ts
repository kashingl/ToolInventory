import { ToolStatus } from '../models/tool.model';

export type ToolStatusColor = 'primary' | 'warn' | 'accent' | '';

const TOOL_STATUS_COLORS: Record<ToolStatus, ToolStatusColor> = {
  Available: 'primary',
  CheckedOut: 'warn',
  UnderMaintenance: 'accent',
  Retired: ''
};

export function getToolStatusColor(status: ToolStatus): ToolStatusColor {
  return TOOL_STATUS_COLORS[status];
}
