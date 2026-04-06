# Data Controls

## HyDataGrid

| 속성 | 기본값 |
|------|--------|
| AutoGenerateColumns | False |
| SelectionUnit | FullRow |
| GridLinesVisibility | All |
| Background | TableRowBg |

내장 스타일: ColumnHeader(SemiBold, TableHeaderBg), Cell(padding 8,4), Row(hover/selected)

| 행 상태 | 배경 |
|---------|------|
| Normal | TableRowBg (#f2f6fa) |
| Hover | TableRowHover (#ebf1f6) |
| Selected | TableRowSelected (#eafaff) |

```xml
<hy:HyDataGrid ItemsSource="{Binding Users}">
    <DataGrid.Columns>
        <DataGridTextColumn Header="이름" Binding="{Binding Name}" />
    </DataGrid.Columns>
</hy:HyDataGrid>
```

## HyTreeView / HyListBox / HyPaginator

```xml
<hy:HyTreeView ItemsSource="{Binding Nodes}">
    <HierarchicalDataTemplate ItemsSource="{Binding Children}">
        <TextBlock Text="{Binding Name}" />
    </HierarchicalDataTemplate>
</hy:HyTreeView>

<hy:HyListBox ItemsSource="{Binding Items}" SelectedItem="{Binding Selected}" />

<hy:HyPaginator TotalItems="{Binding Total}" PageSize="{Binding Size}"
                CurrentPage="{Binding Page}" />
```

## HyBadge

6가지 변형 배지. Padding 6,2 / FontSize 11.

| BadgeType | Fg | Bg |
|-----------|-----|-----|
| Neutral | 기본 | 기본 |
| Info | 파란 | 연한파란 |
| Success | 초록 | 연한초록 |
| Warning | 주황 | 연한주황 |
| Error | 빨강 | 연한빨강 |
| Highlight | 강조 | 강조배경 |

```xml
<hy:HyBadge BadgeType="Success" Content="완료" />
```

하위: [Feedback Controls](01-FeedbackControls.md)
