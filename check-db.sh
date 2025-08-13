#!/bin/bash
echo "=== FAMILIES ==="
sqlite3 my_family_tree_net.db "SELECT Id, Name, CreatedByUserId FROM Families ORDER BY Id DESC LIMIT 5;"
echo ""
echo "=== MEMBERS ==="
sqlite3 my_family_tree_net.db "SELECT Id, FirstName, LastName, FamilyId FROM FamilyMembers ORDER BY Id DESC LIMIT 5;"
echo ""
echo "=== MEMBER COUNT BY FAMILY ==="
sqlite3 my_family_tree_net.db "SELECT f.Id, f.Name, COUNT(fm.Id) as MemberCount FROM Families f LEFT JOIN FamilyMembers fm ON f.Id = fm.FamilyId GROUP BY f.Id ORDER BY f.Id DESC LIMIT 5;"